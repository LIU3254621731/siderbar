namespace FlowDock.App.Services;

public sealed class WindowDetectionResult
{
    public bool Success { get; init; }
    public IntPtr WindowHandle { get; init; }
    public uint ProcessId { get; init; }

    public static WindowDetectionResult Failed { get; } = new()
    {
        Success = false,
        WindowHandle = IntPtr.Zero,
        ProcessId = 0
    };

    public static WindowDetectionResult FromWindow(IntPtr hwnd, uint processId) => new()
    {
        Success = true,
        WindowHandle = hwnd,
        ProcessId = processId
    };
}
