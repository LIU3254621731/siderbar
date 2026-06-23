namespace FlowDock.App.Data;

public interface ISettingsRepository
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}
