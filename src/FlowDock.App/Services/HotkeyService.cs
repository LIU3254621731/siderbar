namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class HotkeyService : IHotkeyService
{
    private readonly ILogger<HotkeyService> _logger;

    public HotkeyService(ILogger<HotkeyService> logger)
    {
        _logger = logger;
    }

    public event EventHandler? HotkeyPressed;

    public Task RegisterAsync(IntPtr hwnd, uint modifiers, uint vk)
    {
        _logger.LogDebug(
            "Registering hotkey on 0x{Handle:X8}: modifiers=0x{Mods:X4} vk=0x{Vk:X4}",
            hwnd, modifiers, vk);

        bool success = NativeMethods.RegisterHotKey(
            hwnd,
            HotkeyConstants.DOCK_HOTKEY_ID,
            modifiers,
            vk);

        if (!success)
        {
            int error = Marshal.GetLastWin32Error();
            _logger.LogError("RegisterHotKey failed with error {Error}", error);
            throw new InvalidOperationException(
                $"Failed to register hotkey. Win32 error: {error}");
        }

        _logger.LogInformation("Hotkey registered successfully");
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(IntPtr hwnd)
    {
        _logger.LogDebug("Unregistering hotkey on 0x{Handle:X8}", hwnd);

        bool success = NativeMethods.UnregisterHotKey(
            hwnd,
            HotkeyConstants.DOCK_HOTKEY_ID);

        if (!success)
        {
            int error = Marshal.GetLastWin32Error();
            _logger.LogWarning("UnregisterHotKey returned false, error {Error}", error);
        }
        else
        {
            _logger.LogInformation("Hotkey unregistered successfully");
        }

        return Task.CompletedTask;
    }

    public Task ApplySettingsAsync(AppSettings settings)
    {
        _logger.LogDebug("Hotkey settings applied: modifiers={Mods}, key={Key}",
            settings.HotkeyModifiers, settings.HotkeyKey);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called by the window message loop when WM_HOTKEY is received.
    /// </summary>
    public void RaiseHotkeyPressed()
    {
        _logger.LogDebug("Hotkey pressed event raised");
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }
}
