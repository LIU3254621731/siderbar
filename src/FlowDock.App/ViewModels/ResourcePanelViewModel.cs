namespace FlowDock.App.ViewModels;

public partial class ResourcePanelViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private CategoryViewModel? _parentCategory;

    public ObservableCollection<ResourceItemViewModel> Items { get; } = new();
}
