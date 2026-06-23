namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;

public interface IDockService
{
    Task InitializeAsync(IntPtr hwnd, AppSettings settings);
    Task ApplySettingsAsync(AppSettings settings);
    void PositionDock(IntPtr hwnd, bool expanded);
    RECT GetWorkingArea();
    void SetAcrylicBackdrop(IntPtr hwnd, AppSettings settings);
}
