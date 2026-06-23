namespace FlowDock.App.Services;

using FlowDock.App.Models;

public sealed class WorkflowRunResult
{
    public bool Success { get; init; }
    public WorkflowRunState State { get; init; } = new();
    public string ErrorMessage { get; init; } = string.Empty;

    public static WorkflowRunResult Succeeded(WorkflowRunState state) => new()
    {
        Success = true,
        State = state
    };

    public static WorkflowRunResult Failed(WorkflowRunState state, string errorMessage) => new()
    {
        Success = false,
        State = state,
        ErrorMessage = errorMessage
    };
}
