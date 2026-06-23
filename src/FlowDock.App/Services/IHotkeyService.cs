namespace FlowDock.App.Services;

public interface IHotkeyService
{
    event EventHandler? HotkeyPressed;

    Task RegisterAsync(IntPtr hwnd, uint modifiers, uint vk);
    Task UnregisterAsync(IntPtr hwnd);
    Task ApplySettingsAsync(AppSettings settings);
}
