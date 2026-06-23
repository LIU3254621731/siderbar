namespace FlowDock.App.Services;

using FlowDock.App.Models;

public interface IThemeService
{
    void ApplyTheme(AppSettings settings);
    void SetDarkMode(bool dark);
}
