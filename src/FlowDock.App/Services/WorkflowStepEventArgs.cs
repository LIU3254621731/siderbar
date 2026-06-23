namespace FlowDock.App.Services;

using FlowDock.App.Models;

public sealed class WorkflowStepEventArgs : EventArgs
{
    public WorkflowStep Step { get; }
    public int StepIndex { get; }

    public WorkflowStepEventArgs(WorkflowStep step, int stepIndex)
    {
        Step = step;
        StepIndex = stepIndex;
    }
}
