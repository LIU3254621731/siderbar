namespace FlowDock.App.Services;

using FlowDock.App.Data;
using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowRepository _workflowRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly IResourceLaunchService _launchService;
    private readonly IProcessTracker _processTracker;
    private readonly IWindowDetectionService _windowDetection;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        IWorkflowRepository workflowRepo,
        IResourceRepository resourceRepo,
        IResourceLaunchService launchService,
        IProcessTracker processTracker,
        IWindowDetectionService windowDetection,
        ILogger<WorkflowEngine> logger)
    {
        _workflowRepo = workflowRepo;
        _resourceRepo = resourceRepo;
        _launchService = launchService;
        _processTracker = processTracker;
        _windowDetection = windowDetection;
        _logger = logger;
    }

    public event EventHandler<WorkflowStepEventArgs>? StepStarted;
    public event EventHandler<WorkflowStepEventArgs>? StepCompleted;
    public event EventHandler<WorkflowRunState>? WorkflowCompleted;

    public Task<WorkflowRunResult> RunWorkflowAsync(
        string workflowId,
        CancellationToken ct = default)
    {
        var visitedIds = new HashSet<string>(StringComparer.Ordinal);
        return RunWorkflowInternalAsync(workflowId, visitedIds, ct);
    }

    private async Task<WorkflowRunResult> RunWorkflowInternalAsync(
        string workflowId,
        HashSet<string> visitedIds,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting workflow: {WorkflowId}", workflowId);

        var state = new WorkflowRunState
        {
            WorkflowId = workflowId,
            Status = WorkflowRunStatus.NotStarted
        };

        // ── Circular reference detection ─────────────────────────
        if (!visitedIds.Add(workflowId))
        {
            string error = $"Circular workflow reference detected: {workflowId}";
            _logger.LogError(error);
            state.Status = WorkflowRunStatus.Failed;
            state.ErrorMessage = error;
            var failResult = WorkflowRunResult.Failed(state, error);
            WorkflowCompleted?.Invoke(this, state);
            return failResult;
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            // ── Load workflow definition ──────────────────────────
            var workflow = await _workflowRepo.GetByIdAsync(workflowId);
            if (workflow is null)
            {
                string error = $"Workflow not found: {workflowId}";
                _logger.LogError(error);
                state.Status = WorkflowRunStatus.Failed;
                state.ErrorMessage = error;
                var failResult = WorkflowRunResult.Failed(state, error);
                WorkflowCompleted?.Invoke(this, state);
                return failResult;
            }

            // ── Validate all resources exist ──────────────────────
            var enabledSteps = workflow.Steps
                .Where(s => s.IsEnabled)
                .OrderBy(s => s.StepNumber)
                .ToList();

            foreach (var step in enabledSteps)
            {
                if (!string.IsNullOrWhiteSpace(step.ResourceId))
                {
                    var resource = await _resourceRepo.GetByIdAsync(step.ResourceId);
                    if (resource is null)
                    {
                        string error = $"Resource '{step.ResourceId}' not found for step {step.StepNumber} in workflow '{workflow.Name}'";
                        _logger.LogError(error);
                        state.Status = WorkflowRunStatus.Failed;
                        state.ErrorMessage = error;
                        var failResult = WorkflowRunResult.Failed(state, error);
                        WorkflowCompleted?.Invoke(this, state);
                        return failResult;
                    }
                }
            }

            // ── Execute steps ─────────────────────────────────────
            state.Status = WorkflowRunStatus.Running;

            for (int i = 0; i < enabledSteps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var step = enabledSteps[i];
                state.CurrentStepIndex = i;

                _logger.LogDebug(
                    "Executing step {Index}/{Total}: number={StepNumber}, resourceId={ResourceId}",
                    i, enabledSteps.Count, step.StepNumber, step.ResourceId);

                StepStarted?.Invoke(this, new WorkflowStepEventArgs(step, i));

                try
                {
                    var resource = await _resourceRepo.GetByIdAsync(step.ResourceId);
                    if (resource is null)
                    {
                        throw new InvalidOperationException(
                            $"Resource '{step.ResourceId}' vanished during execution");
                    }

                    Process? launchedProcess = null;

                    // ── Handle nested workflow ────────────────────
                    if (resource.Type == ResourceType.Workflow)
                    {
                        if (string.IsNullOrWhiteSpace(resource.WorkflowId))
                        {
                            throw new InvalidOperationException(
                                $"Workflow resource '{resource.Name}' has no WorkflowId");
                        }

                        _logger.LogInformation(
                            "Dispatching nested workflow: {WorkflowId} from resource '{ResourceName}'",
                            resource.WorkflowId, resource.Name);

                        var nestedResult = await RunWorkflowInternalAsync(
                            resource.WorkflowId, visitedIds, ct);

                        if (!nestedResult.Success)
                        {
                            if (workflow.StopOnFailure)
                            {
                                state.Status = WorkflowRunStatus.Failed;
                                state.ErrorMessage = $"Nested workflow failed: {nestedResult.ErrorMessage}";
                                var failResult = WorkflowRunResult.Failed(state, state.ErrorMessage);
                                WorkflowCompleted?.Invoke(this, state);
                                return failResult;
                            }

                            _logger.LogWarning(
                                "Nested workflow failed but StopOnFailure is false, continuing");
                        }

                        // Merge nested processes into current state
                        state.LaunchedProcesses.AddRange(nestedResult.State.LaunchedProcesses);
                    }
                    else
                    {
                        // ── Launch resource ───────────────────────
                        launchedProcess = await _launchService.LaunchAsync(resource);

                        if (launchedProcess is not null)
                        {
                            var tracked = _processTracker.RegisterProcess(
                                launchedProcess,
                                resource.Id,
                                workflowId);

                            state.LaunchedProcesses.Add(tracked);
                        }

                        // ── Wait for window if required ──────────
                        if (step.WaitForWindow)
                        {
                            state.Status = WorkflowRunStatus.WaitingForWindow;

                            var timeout = step.WaitTimeoutMs > 0
                                ? TimeSpan.FromMilliseconds(step.WaitTimeoutMs)
                                : workflow.StepTimeout;

                            var detectionResult = await _windowDetection.WaitForWindowAsync(
                                step.WindowTitlePattern,
                                step.WindowClassName,
                                timeout,
                                ct);

                            if (!detectionResult.Success)
                            {
                                string error = string.IsNullOrWhiteSpace(step.FailureMessage)
                                    ? $"Window wait timed out for step {step.StepNumber}: title='{step.WindowTitlePattern}', class='{step.WindowClassName}'"
                                    : step.FailureMessage;

                                _logger.LogWarning(error);

                                if (workflow.StopOnFailure)
                                {
                                    state.Status = WorkflowRunStatus.Failed;
                                    state.ErrorMessage = error;
                                    var failResult = WorkflowRunResult.Failed(state, error);
                                    WorkflowCompleted?.Invoke(this, state);
                                    return failResult;
                                }
                            }
                        }

                        state.Status = WorkflowRunStatus.StepCompleted;
                    }

                    StepCompleted?.Invoke(this, new WorkflowStepEventArgs(step, i));
                }
                catch (OperationCanceledException)
                {
                    state.Status = WorkflowRunStatus.Cancelled;
                    var cancelResult = WorkflowRunResult.Failed(state, "Workflow cancelled");
                    WorkflowCompleted?.Invoke(this, state);
                    return cancelResult;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Step {StepNumber} failed in workflow '{WorkflowName}'",
                        step.StepNumber, workflow.Name);

                    if (workflow.StopOnFailure)
                    {
                        string error = string.IsNullOrWhiteSpace(step.FailureMessage)
                            ? $"Step {step.StepNumber} failed: {ex.Message}"
                            : step.FailureMessage;

                        state.Status = WorkflowRunStatus.Failed;
                        state.ErrorMessage = error;
                        var failResult = WorkflowRunResult.Failed(state, error);
                        WorkflowCompleted?.Invoke(this, state);
                        return failResult;
                    }

                    _logger.LogWarning("Step failed but StopOnFailure is false, continuing");
                }
            }

            // ── All steps completed ──────────────────────────────
            state.Status = WorkflowRunStatus.Completed;
            _logger.LogInformation(
                "Workflow '{WorkflowName}' completed successfully ({Count} processes launched)",
                workflow.Name, state.LaunchedProcesses.Count);

            var successResult = WorkflowRunResult.Succeeded(state);
            WorkflowCompleted?.Invoke(this, state);
            return successResult;
        }
        catch (OperationCanceledException)
        {
            state.Status = WorkflowRunStatus.Cancelled;
            var cancelResult = WorkflowRunResult.Failed(state, "Workflow cancelled");
            WorkflowCompleted?.Invoke(this, state);
            return cancelResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error running workflow {WorkflowId}", workflowId);
            state.Status = WorkflowRunStatus.Failed;
            state.ErrorMessage = $"Unexpected error: {ex.Message}";
            var failResult = WorkflowRunResult.Failed(state, state.ErrorMessage);
            WorkflowCompleted?.Invoke(this, state);
            return failResult;
        }
    }
}
