namespace FlowDock.App.Models;

public enum WorkflowRunStatus
{
    NotStarted,
    Running,
    WaitingForWindow,
    StepCompleted,
    Completed,
    Failed,
    Cancelled
}
