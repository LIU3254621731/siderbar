namespace FlowDock.App.ViewModels;

public partial class DockContainerViewModel : ObservableObject
{
    [ObservableProperty]
    private double _dockWidth = 320;

    [ObservableProperty]
    private double _dockCollapsedWidth = 48;

    [ObservableProperty]
    private string _dockEdge = "Left";

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private double _animationDuration = 250;

    [RelayCommand]
    private void Toggle()
    {
        IsExpanded = !IsExpanded;
    }

    [RelayCommand]
    private void Expand()
    {
        IsExpanded = true;
    }

    [RelayCommand]
    private void Collapse()
    {
        IsExpanded = false;
    }

    public void OnEdgeTriggered()
    {
        IsExpanded = true;
    }

    public void OnPointerLeft()
    {
        IsExpanded = false;
    }

    public void OnSettingsRequested()
    {
        // Placeholder: open settings window
    }
}
