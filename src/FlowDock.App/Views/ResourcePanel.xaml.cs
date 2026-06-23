namespace FlowDock.App.Views;

public sealed class ResourcePanel : UserControl
{
    public ResourcePanelViewModel ViewModel { get; }
    private readonly StackPanel _itemsPanel;

    public ResourcePanel()
    {
        ViewModel = new ResourcePanelViewModel();

        _itemsPanel = new StackPanel { Spacing = 2 };

        Content = _itemsPanel;
    }

    public void BindViewModel(ResourcePanelViewModel viewModel)
    {
        this.DataContext = viewModel;
        viewModel.Items.CollectionChanged += (_, _) => RefreshItems();
        RefreshItems();
    }

    public async Task ShowAsync()
    {
        Visibility = Visibility.Visible;
        Opacity = 1.0;
        await Task.CompletedTask;
    }

    public async Task HideAsync()
    {
        await Task.Delay(150);
        Visibility = Visibility.Collapsed;
        Opacity = 0.0;
    }

    private void RefreshItems()
    {
        _itemsPanel.Children.Clear();
        foreach (var itemVm in ViewModel.Items)
        {
            var tile = new ResourceTile();
            tile.BindViewModel(itemVm);
            _itemsPanel.Children.Add(tile);
        }
    }
}
