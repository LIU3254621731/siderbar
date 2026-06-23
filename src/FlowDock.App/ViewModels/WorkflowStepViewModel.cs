namespace FlowDock.App.ViewModels;

public partial class WorkflowStepViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepNumberText))]
    private int _stepNumber;

    public string StepNumberText => $"Step {StepNumber}";

    [ObservableProperty]
    private string _resourceId = string.Empty;

    public ResourceItem? SelectedResource
    {
        get => AvailableResources.FirstOrDefault(r => r.Id == ResourceId);
        set
        {
            if (value is not null)
            {
                ResourceId = value.Id;
            }
        }
    }

    [ObservableProperty]
    private bool _waitForWindow;

    [ObservableProperty]
    private string _windowTitlePattern = string.Empty;

    [ObservableProperty]
    private string _windowClassName = string.Empty;

    [ObservableProperty]
    private int _waitTimeoutMs = 30000;

    [ObservableProperty]
    private bool _isEnabled = true;

    public ObservableCollection<ResourceItem> AvailableResources { get; } = new();

    public async Task LoadAvailableResourcesAsync(IDataRepository dataRepository)
    {
        var resources = await dataRepository.Resources.GetAllAsync();
        AvailableResources.Clear();
        foreach (var resource in resources)
        {
            AvailableResources.Add(resource);
        }
    }
}
