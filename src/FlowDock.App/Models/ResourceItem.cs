namespace FlowDock.App.Models;

public class ResourceItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public string IconGlyph { get; set; } = "";

    public string TargetPath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public SystemActionType? SystemAction { get; set; }
    public string WorkflowId { get; set; } = string.Empty;

    public string CategoryId { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool RunAsAdmin { get; set; }
    public string WorkingDirectory { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
