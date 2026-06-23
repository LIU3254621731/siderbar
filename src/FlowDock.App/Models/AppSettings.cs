namespace FlowDock.App.Models;

public class AppSettings
{
    public InteractionMode InteractionMode { get; set; } = InteractionMode.PureHover;
    public string HotkeyModifiers { get; set; } = "Control+Alt";
    public string HotkeyKey { get; set; } = "D";
    public int HotkeyVirtualKey { get; set; } = 0x44;
    public int HotkeyModifiersCode { get; set; } = 0x0003;

    public int DockWidth { get; set; } = 320;
    public int DockCollapsedWidth { get; set; } = 48;
    public int DockAnimationDurationMs { get; set; } = 250;
    public string DockEdge { get; set; } = "Left";

    public bool UseAcrylic { get; set; } = true;
    public bool UseFrostedGlass { get; set; } = true;
    public int BlurIntensity { get; set; } = 80;
    public bool FollowSystemTheme { get; set; } = true;
    public string CustomAccentColor { get; set; } = string.Empty;

    public int TargetMonitorIndex { get; set; } = 0;

    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowOnAllDesktops { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
