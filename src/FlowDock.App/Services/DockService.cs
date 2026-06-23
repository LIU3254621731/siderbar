namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class DockService : IDockService
{
    private const int WCA_ACCENT_POLICY = 19;

    private readonly ILogger<DockService> _logger;
    private AppSettings? _settings;

    public DockService(ILogger<DockService> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync(IntPtr hwnd, AppSettings settings)
    {
        _logger.LogDebug("Initializing dock service for window 0x{Handle:X8}", hwnd);
        _settings = settings;

        SetAcrylicBackdrop(hwnd, settings);

        if (settings.DockEdge is not ("Left" or "Right"))
        {
            _logger.LogWarning("Unsupported DockEdge '{Edge}', defaulting to Left", settings.DockEdge);
            settings.DockEdge = "Left";
        }

        PositionDock(hwnd, expanded: false);
        return Task.CompletedTask;
    }

    public void PositionDock(IntPtr hwnd, bool expanded)
    {
        var workArea = GetWorkingArea();
        var settings = _settings ?? new AppSettings();

        int width = expanded ? settings.DockWidth : settings.DockCollapsedWidth;
        int x;
        int y = workArea.Top;
        int height = workArea.Height;

        if (settings.DockEdge == "Right")
        {
            x = workArea.Right - width;
        }
        else
        {
            x = workArea.Left;
        }

        _logger.LogDebug(
            "Positioning dock: x={X}, y={Y}, w={Width}, h={Height}, edge={Edge}, expanded={Expanded}",
            x, y, width, height, settings.DockEdge, expanded);

        NativeMethods.SetWindowPos(
            hwnd,
            WindowStyles.HWND_TOPMOST,
            x,
            y,
            width,
            height,
            WindowStyles.SWP_NOACTIVATE | WindowStyles.SWP_SHOWWINDOW);
    }

    public RECT GetWorkingArea()
    {
        var rect = new RECT();
        NativeMethods.SystemParametersInfo(
            WindowStyles.SPI_GETWORKAREA,
            0,
            ref rect,
            0);
        return rect;
    }

    public Task ApplySettingsAsync(AppSettings settings)
    {
        _settings = settings;
        return Task.CompletedTask;
    }

    public void SetAcrylicBackdrop(IntPtr hwnd, AppSettings settings)
    {
        if (!settings.UseAcrylic)
        {
            _logger.LogDebug("Acrylic backdrop disabled via settings");
            return;
        }

        bool success = TrySetAccent(hwnd, AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND);

        if (!success)
        {
            _logger.LogWarning(
                "ACCENT_ENABLE_ACRYLICBLURBEHIND failed, falling back to ACCENT_ENABLE_BLURBEHIND");
            TrySetAccent(hwnd, AccentState.ACCENT_ENABLE_BLURBEHIND);
        }
    }

    private bool TrySetAccent(IntPtr hwnd, AccentState accentState)
    {
        try
        {
            var accent = new AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = 2,
                GradientColor = 0,
                AnimationId = 0
            };

            int structSize = Marshal.SizeOf<AccentPolicy>();
            IntPtr accentPtr = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WCA_ACCENT_POLICY,
                Data = accentPtr,
                SizeOfData = structSize
            };

            bool result = NativeMethods.SetWindowCompositionAttribute(hwnd, ref data);
            int error = Marshal.GetLastWin32Error();
            Marshal.FreeHGlobal(accentPtr);

            if (!result)
            {
                _logger.LogWarning(
                    "SetWindowCompositionAttribute failed with error {Error} for state {State}",
                    error, accentState);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while setting window accent to {State}", accentState);
            return false;
        }
    }
}
