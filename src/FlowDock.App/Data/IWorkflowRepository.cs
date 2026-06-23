namespace FlowDock.App.Data;

public interface IWorkflowRepository
{
    Task<WorkflowDefinition?> GetByIdAsync(string id);
    Task<IReadOnlyList<WorkflowDefinition>> GetAllAsync();
    Task<WorkflowDefinition> AddAsync(WorkflowDefinition workflow);
    Task<WorkflowDefinition> UpdateAsync(WorkflowDefinition workflow);
    Task<bool> DeleteAsync(string id);
}
