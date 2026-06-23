namespace FlowDock.App.Services;

using FlowDock.App.Models;

public interface IWorkflowEngine
{
    Task<WorkflowRunResult> RunWorkflowAsync(
        string workflowId,
        CancellationToken ct = default);

    event EventHandler<WorkflowStepEventArgs>? StepStarted;
    event EventHandler<WorkflowStepEventArgs>? StepCompleted;
    event EventHandler<WorkflowRunState>? WorkflowCompleted;
}
