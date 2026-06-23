namespace FlowDock.App.Services;

using FlowDock.App.Models;

public interface IProcessTracker
{
    TrackedProcess RegisterProcess(Process process, string resourceId, string workflowId);
    void UnregisterProcess(int processId);
    IReadOnlyList<TrackedProcess> GetProcessesForWorkflow(string workflowId);
    Task CloseAllForWorkflowAsync(string workflowId);

    event EventHandler<ProcessExitedEventArgs>? ProcessExited;
}
