using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace FlowDock.App.Views;

public sealed class DockContainer : UserControl
{
    public DockContainerViewModel ViewModel { get; }

    private readonly Compositor _compositor;
    private Grid _rootGrid = null!;
    private ScrollViewer _scrollViewer = null!;
    private CategoryList _categoryList = null!;

    private bool _isExpanded;
    private bool _isHovering;

    public DockContainer()
    {
        ViewModel = App.Host.Services.GetRequiredService<DockContainerViewModel>();
        _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        BuildUI();
    }

    private void BuildUI()
    {
        _rootGrid = new Grid
        {
            Width = ViewModel.DockWidth,
            MinWidth = ViewModel.DockCollapsedWidth,
            Background = new SolidColorBrush(Color.FromArgb(0xE6, 0x22, 0x22, 0x33)),
            CornerRadius = new CornerRadius(8)
        };

        _rootGrid.PointerEntered += OnPointerEntered;
        _rootGrid.PointerExited += OnPointerExited;

        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _categoryList = new CategoryList();
        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _categoryList
        };
        Grid.SetRow(_scrollViewer, 1);
        _rootGrid.Children.Add(_scrollViewer);

        this.Content = _rootGrid;
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
        if (!_isExpanded) ViewModel.OnEdgeTriggered();
    }

    private async void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = false;
        await Task.Delay(300);
        if (!_isHovering && _isExpanded) ViewModel.OnPointerLeft();
    }
}
