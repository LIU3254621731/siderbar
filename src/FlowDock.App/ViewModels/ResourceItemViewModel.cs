namespace FlowDock.App.ViewModels;

public partial class ResourceItemViewModel : ObservableObject
{
    private readonly ResourceItem? _resource;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _iconGlyph = string.Empty;

    [ObservableProperty]
    private ResourceType _type;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Ready";

    public ResourceItemViewModel()
    {
    }

    public ResourceItemViewModel(ResourceItem resource)
    {
        _resource = resource;
        Id = resource.Id;
        Name = resource.Name;
        Description = resource.Description;
        IconGlyph = resource.IconGlyph;
        Type = resource.Type;
    }

    [RelayCommand]
    private async Task Launch()
    {
        // TODO: Resolve IResourceLaunchService from DI and call LaunchAsync(_resource)
        StatusText = "Launching...";
        IsRunning = true;
        await Task.CompletedTask;
        StatusText = "Running";
    }

    [RelayCommand]
    private void Edit()
    {
        // Placeholder for P4 — will open resource edit dialog
    }

    [RelayCommand]
    private void Delete()
    {
        // Placeholder for P4 — will confirm and delete resource
    }
}
