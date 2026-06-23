namespace FlowDock.App.Data;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string id);
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<Category> AddAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
}
