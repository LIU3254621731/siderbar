namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using Microsoft.Extensions.Logging;

public sealed class WindowDetectionService : IWindowDetectionService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(250);

    private readonly ILogger<WindowDetectionService> _logger;

    public WindowDetectionService(ILogger<WindowDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<WindowDetectionResult> WaitForWindowAsync(
        string titlePattern,
        string className,
        TimeSpan timeout,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "Waiting for window: title='{Title}', class='{Class}', timeout={Timeout}ms",
            titlePattern, className, timeout.TotalMilliseconds);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                var handles = await FindWindowsAsync(titlePattern, className);

                if (handles.Count > 0)
                {
                    IntPtr hwnd = handles[0];
                    uint processId = GetWindowProcessId(hwnd);

                    _logger.LogInformation(
                        "Window found: 0x{Handle:X8}, pid={Pid}",
                        hwnd, processId);

                    return WindowDetectionResult.FromWindow(hwnd, processId);
                }

                await Task.Delay(PollingInterval, linkedCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            bool wasTimeout = timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested;

            _logger.LogWarning(
                wasTimeout
                    ? "Window wait timed out after {Timeout}ms: title='{Title}', class='{Class}'"
                    : "Window wait cancelled: title='{Title}', class='{Class}'",
                timeout.TotalMilliseconds, titlePattern, className);
        }

        return WindowDetectionResult.Failed;
    }

    public Task<IReadOnlyList<IntPtr>> FindWindowsAsync(
        string titlePattern,
        string className)
    {
        var results = new List<IntPtr>();

        NativeMethods.EnumWindows((hwnd, lParam) =>
        {
            if (!NativeMethods.IsWindowVisible(hwnd))
                return true; // continue enumeration

            string windowTitle = GetWindowText(hwnd);
            string windowClass = GetWindowClassName(hwnd);

            bool titleMatch = string.IsNullOrWhiteSpace(titlePattern)
                || windowTitle.Contains(titlePattern, StringComparison.OrdinalIgnoreCase);

            bool classMatch = string.IsNullOrWhiteSpace(className)
                || string.Equals(windowClass, className, StringComparison.OrdinalIgnoreCase);

            if (titleMatch || classMatch)
            {
                results.Add(hwnd);
            }

            return true; // continue enumeration
        }, IntPtr.Zero);

        return Task.FromResult<IReadOnlyList<IntPtr>>(results);
    }

    private static string GetWindowText(IntPtr hwnd)
    {
        var sb = new StringBuilder(512);
        NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetWindowClassName(IntPtr hwnd)
    {
        var sb = new StringBuilder(512);
        NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static uint GetWindowProcessId(IntPtr hwnd)
    {
        NativeMethods.GetWindowThreadProcessId(hwnd, out uint processId);
        return processId;
    }
}
