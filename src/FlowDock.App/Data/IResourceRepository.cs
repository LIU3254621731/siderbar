namespace FlowDock.App.Data;

public interface IResourceRepository
{
    Task<ResourceItem?> GetByIdAsync(string id);
    Task<IReadOnlyList<ResourceItem>> GetByCategoryAsync(string categoryId);
    Task<IReadOnlyList<ResourceItem>> GetAllAsync();
    Task<ResourceItem> AddAsync(ResourceItem resource);
    Task<ResourceItem> UpdateAsync(ResourceItem resource);
    Task<bool> DeleteAsync(string id);
}
