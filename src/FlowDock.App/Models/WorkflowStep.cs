namespace FlowDock.App.Models;

public class WorkflowStep
{
    public int StepNumber { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public bool WaitForWindow { get; set; }
    public string WindowTitlePattern { get; set; } = string.Empty;
    public string WindowClassName { get; set; } = string.Empty;
    public int WaitTimeoutMs { get; set; } = 30000;
    public bool IsEnabled { get; set; } = true;
    public string FailureMessage { get; set; } = string.Empty;
}
