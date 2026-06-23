namespace FlowDock.App.Models;

public class TrackedProcess
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public IntPtr MainWindowHandle { get; set; }
}
