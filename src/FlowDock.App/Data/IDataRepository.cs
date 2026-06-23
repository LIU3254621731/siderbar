namespace FlowDock.App.Data;

public interface IDataRepository
{
    ICategoryRepository Categories { get; }
    IResourceRepository Resources { get; }
    IWorkflowRepository Workflows { get; }
    ISettingsRepository Settings { get; }

    Task LoadAllAsync();
    Task SaveAllAsync();
}
