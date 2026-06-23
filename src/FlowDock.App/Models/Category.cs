namespace FlowDock.App.Models;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "";
    public int SortOrder { get; set; }
    public string ColorHint { get; set; } = string.Empty;
    public List<string> ResourceIds { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
