namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private bool _isDarkMode;

    public bool IsDarkMode => _isDarkMode;

    public ThemeService(ILogger<ThemeService> logger)
    {
        _logger = logger;
    }

    public void ApplyTheme(AppSettings settings)
    {
        _logger.LogInformation(
            "Applying theme: DarkMode={DarkMode}, UseAcrylic={Acrylic}, BlurIntensity={Blur}",
            _isDarkMode, settings.UseAcrylic, settings.BlurIntensity);

        // For MVP, theme state is primarily stored and relayed.
        // The acrylic backdrop is configured by IDockService when the window
        // is initialized. This service acts as the centralized theme authority
        // that view models and views can query.
    }

    public void SetDarkMode(bool dark)
    {
        _logger.LogInformation("Setting dark mode: {Dark}", dark);
        _isDarkMode = dark;
    }
}
