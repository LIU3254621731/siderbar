namespace FlowDock.App.Data.Json;

public class JsonResourceRepository : IResourceRepository
{
    private readonly JsonDataStore _store;
    private List<ResourceItem> _cache = new();
    private const string FileName = "resources.json";

    public JsonResourceRepository(JsonDataStore store)
    {
        _store = store;
    }

    public async Task LoadAsync()
    {
        _cache = await _store.LoadFileAsync<List<ResourceItem>>(FileName);
    }

    public async Task SaveAsync()
    {
        await _store.SaveFileAsync(FileName, _cache);
    }

    public Task<ResourceItem?> GetByIdAsync(string id)
    {
        return Task.FromResult(_cache.FirstOrDefault(r => r.Id == id));
    }

    public Task<IReadOnlyList<ResourceItem>> GetByCategoryAsync(string categoryId)
    {
        return Task.FromResult<IReadOnlyList<ResourceItem>>(
            _cache.Where(r => r.CategoryId == categoryId).OrderBy(r => r.SortOrder).ToList());
    }

    public Task<IReadOnlyList<ResourceItem>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<ResourceItem>>(_cache.OrderBy(r => r.Name).ToList());
    }

    public async Task<ResourceItem> AddAsync(ResourceItem resource)
    {
        resource.CreatedAt = DateTime.UtcNow;
        resource.UpdatedAt = DateTime.UtcNow;
        _cache.Add(resource);
        await SaveAsync();
        return resource;
    }

    public async Task<ResourceItem> UpdateAsync(ResourceItem resource)
    {
        var index = _cache.FindIndex(r => r.Id == resource.Id);
        if (index < 0) throw new InvalidOperationException($"Resource '{resource.Id}' not found");
        resource.UpdatedAt = DateTime.UtcNow;
        _cache[index] = resource;
        await SaveAsync();
        return resource;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _cache.RemoveAll(r => r.Id == id);
        if (removed > 0) await SaveAsync();
        return removed > 0;
    }
}
