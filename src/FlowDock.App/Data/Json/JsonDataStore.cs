namespace FlowDock.App.Data.Json;

public class JsonDataStore : IDataRepository
{
    private readonly string _dataDir;
    private readonly string _backupDir;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly ILogger<JsonDataStore> _logger;

    private JsonCategoryRepository? _categories;
    private JsonResourceRepository? _resources;
    private JsonWorkflowRepository? _workflows;
    private JsonSettingsRepository? _settings;

    public ICategoryRepository Categories => _categories!;
    public IResourceRepository Resources => _resources!;
    public IWorkflowRepository Workflows => _workflows!;
    public ISettingsRepository Settings => _settings!;

    public JsonDataStore(ILogger<JsonDataStore> logger)
    {
        _logger = logger;
        _dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FlowDock", "data");
        _backupDir = Path.Combine(_dataDir, "backups");

        Directory.CreateDirectory(_dataDir);
        Directory.CreateDirectory(_backupDir);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _categories = new JsonCategoryRepository(this);
        _resources = new JsonResourceRepository(this);
        _workflows = new JsonWorkflowRepository(this);
        _settings = new JsonSettingsRepository(this);
    }

    public async Task LoadAllAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            await _categories!.LoadAsync();
            await _resources!.LoadAsync();
            await _workflows!.LoadAsync();
            await _settings!.LoadAsync();
            _logger.LogInformation("All data loaded from {DataDir}", _dataDir);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveAllAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            await _categories!.SaveAsync();
            await _resources!.SaveAsync();
            await _workflows!.SaveAsync();
            await _settings!.SaveAsync();
            _logger.LogInformation("All data saved to {DataDir}", _dataDir);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveFileAsync<T>(string fileName, T data)
    {
        var filePath = Path.Combine(_dataDir, fileName);
        var tmpPath = filePath + ".tmp";

        await _fileLock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(tmpPath, json);

            if (File.Exists(filePath))
            {
                var backupName = $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                var backupPath = Path.Combine(_backupDir, backupName);
                File.Copy(filePath, backupPath, overwrite: true);
            }

            File.Move(tmpPath, filePath, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {FileName}", fileName);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<T> LoadFileAsync<T>(string fileName) where T : new()
    {
        var filePath = Path.Combine(_dataDir, fileName);

        await _fileLock.WaitAsync();
        try
        {
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }

            var backupFiles = Directory.Exists(_backupDir)
                ? Directory.GetFiles(_backupDir, $"{Path.GetFileNameWithoutExtension(fileName)}-*.json")
                    .OrderByDescending(f => f)
                    .ToList()
                : new List<string>();

            if (backupFiles.Count > 0)
            {
                _logger.LogWarning("Main file {FileName} missing, loading from backup {Backup}", fileName, backupFiles[0]);
                var json = await File.ReadAllTextAsync(backupFiles[0]);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }

            _logger.LogInformation("No data file {FileName}, returning default", fileName);
            return new T();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load {FileName}, returning default", fileName);
            return new T();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task WaitForLockAsync()
    {
        await _fileLock.WaitAsync();
    }

    public void ReleaseLock()
    {
        _fileLock.Release();
    }
}
