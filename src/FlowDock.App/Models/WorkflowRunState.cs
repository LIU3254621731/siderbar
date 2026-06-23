namespace FlowDock.App.Models;

public class WorkflowRunState
{
    public string WorkflowRunId { get; set; } = Guid.NewGuid().ToString("N");
    public string WorkflowId { get; set; } = string.Empty;
    public int CurrentStepIndex { get; set; }
    public WorkflowRunStatus Status { get; set; } = WorkflowRunStatus.NotStarted;
    public List<TrackedProcess> LaunchedProcesses { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public string ErrorMessage { get; set; } = string.Empty;
}
