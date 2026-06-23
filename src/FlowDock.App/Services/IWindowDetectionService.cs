namespace FlowDock.App.Services;

public interface IWindowDetectionService
{
    Task<WindowDetectionResult> WaitForWindowAsync(
        string titlePattern,
        string className,
        TimeSpan timeout,
        CancellationToken ct);

    Task<IReadOnlyList<IntPtr>> FindWindowsAsync(
        string titlePattern,
        string className);
}
