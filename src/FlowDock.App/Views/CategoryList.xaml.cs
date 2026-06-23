namespace FlowDock.App.Views;

public sealed class CategoryList : UserControl
{
    public MainViewModel ViewModel { get; }
    private readonly StackPanel _categoriesPanel;

    public CategoryList()
    {
        ViewModel = App.Host.Services.GetRequiredService<MainViewModel>();

        _categoriesPanel = new StackPanel
        {
            Spacing = 4,
            Padding = new Thickness(0, 8, 0, 8)
        };

        ViewModel.Categories.CollectionChanged += (_, _) => RefreshCategories();

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _categoriesPanel
        };

        RefreshCategories();
    }

    private void RefreshCategories()
    {
        _categoriesPanel.Children.Clear();
        foreach (var categoryVm in ViewModel.Categories)
        {
            var tab = new CategoryTab();
            tab.BindViewModel(categoryVm);
            _categoriesPanel.Children.Add(tab);
        }
    }
}
