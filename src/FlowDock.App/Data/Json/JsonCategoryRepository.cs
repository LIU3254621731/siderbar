namespace FlowDock.App.Data.Json;

public class JsonCategoryRepository : ICategoryRepository
{
    private readonly JsonDataStore _store;
    private List<Category> _cache = new();
    private const string FileName = "categories.json";

    public JsonCategoryRepository(JsonDataStore store)
    {
        _store = store;
    }

    public async Task LoadAsync()
    {
        _cache = await _store.LoadFileAsync<List<Category>>(FileName);
    }

    public async Task SaveAsync()
    {
        await _store.SaveFileAsync(FileName, _cache);
    }

    public Task<Category?> GetByIdAsync(string id)
    {
        return Task.FromResult(_cache.FirstOrDefault(c => c.Id == id));
    }

    public Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Category>>(_cache.OrderBy(c => c.SortOrder).ToList());
    }

    public async Task<Category> AddAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        _cache.Add(category);
        await SaveAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        var index = _cache.FindIndex(c => c.Id == category.Id);
        if (index < 0) throw new InvalidOperationException($"Category '{category.Id}' not found");
        category.UpdatedAt = DateTime.UtcNow;
        _cache[index] = category;
        await SaveAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _cache.RemoveAll(c => c.Id == id);
        if (removed > 0) await SaveAsync();
        return removed > 0;
    }
}
