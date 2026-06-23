namespace FlowDock.App.Data.Json;

public class JsonWorkflowRepository : IWorkflowRepository
{
    private readonly JsonDataStore _store;
    private List<WorkflowDefinition> _cache = new();
    private const string FileName = "workflows.json";

    public JsonWorkflowRepository(JsonDataStore store)
    {
        _store = store;
    }

    public async Task LoadAsync()
    {
        _cache = await _store.LoadFileAsync<List<WorkflowDefinition>>(FileName);
    }

    public async Task SaveAsync()
    {
        await _store.SaveFileAsync(FileName, _cache);
    }

    public Task<WorkflowDefinition?> GetByIdAsync(string id)
    {
        return Task.FromResult(_cache.FirstOrDefault(w => w.Id == id));
    }

    public Task<IReadOnlyList<WorkflowDefinition>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<WorkflowDefinition>>(_cache.OrderBy(w => w.Name).ToList());
    }

    public async Task<WorkflowDefinition> AddAsync(WorkflowDefinition workflow)
    {
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;
        _cache.Add(workflow);
        await SaveAsync();
        return workflow;
    }

    public async Task<WorkflowDefinition> UpdateAsync(WorkflowDefinition workflow)
    {
        var index = _cache.FindIndex(w => w.Id == workflow.Id);
        if (index < 0) throw new InvalidOperationException($"Workflow '{workflow.Id}' not found");
        workflow.UpdatedAt = DateTime.UtcNow;
        _cache[index] = workflow;
        await SaveAsync();
        return workflow;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _cache.RemoveAll(w => w.Id == id);
        if (removed > 0) await SaveAsync();
        return removed > 0;
    }
}
