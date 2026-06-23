namespace FlowDock.App.Data.Json;

public class JsonSettingsRepository : ISettingsRepository
{
    private readonly JsonDataStore _store;
    private AppSettings _settings = new();
    private const string FileName = "settings.json";

    public JsonSettingsRepository(JsonDataStore store)
    {
        _store = store;
    }

    public async Task LoadAsync()
    {
        _settings = await _store.LoadFileAsync<AppSettings>(FileName);
    }

    public async Task SaveAsync()
    {
        await _store.SaveFileAsync(FileName, _settings);
    }

    public Task<AppSettings> GetSettingsAsync()
    {
        return Task.FromResult(_settings);
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _settings = settings;
        await SaveAsync();
    }
}
