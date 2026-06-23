namespace FlowDock.App.Views;

public sealed class CategoryTab : UserControl
{
    public CategoryViewModel ViewModel { get; private set; } = null!;

    private readonly Grid _headerGrid;
    private readonly TextBlock _iconText;
    private readonly TextBlock _nameText;
    private readonly TextBlock _expandIndicator;
    private readonly StackPanel _resourcesPanel;
    private readonly StackPanel _expandArea;

    private static readonly SolidColorBrush HoverBrush =
        new(Windows.UI.Color.FromArgb(30, 255, 255, 255));

    private static readonly SolidColorBrush RestBrush =
        new(Colors.Transparent);

    private static readonly SolidColorBrush ActiveBrush =
        new(Windows.UI.Color.FromArgb(60, 96, 140, 255));

    public CategoryTab()
    {
        _headerGrid = new Grid
        {
            Height = 40,
            Padding = new Thickness(12, 0, 8, 0),
            CornerRadius = new CornerRadius(6),
            Background = RestBrush
        };
        _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
        _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _iconText = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = new FontFamily("Segoe Fluent Icons"),
            Text = ""
        };
        Grid.SetColumn(_iconText, 0);
        _headerGrid.Children.Add(_iconText);

        _nameText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            FontSize = 13,
            Foreground = new SolidColorBrush(Colors.White)
        };
        Grid.SetColumn(_nameText, 1);
        _headerGrid.Children.Add(_nameText);

        _expandIndicator = new TextBlock
        {
            Text = "",
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Segoe Fluent Icons"),
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(180, 255, 255, 255))
        };
        Grid.SetColumn(_expandIndicator, 2);
        _headerGrid.Children.Add(_expandIndicator);

        _resourcesPanel = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(20, 4, 4, 4)
        };

        _expandArea = new StackPanel
        {
            Children = { _resourcesPanel },
            Visibility = Visibility.Collapsed
        };

        var rootGrid = new Grid();
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(_headerGrid, 0);
        Grid.SetRow(_expandArea, 1);
        rootGrid.Children.Add(_headerGrid);
        rootGrid.Children.Add(_expandArea);

        _headerGrid.PointerEntered += OnHeaderPointerEntered;
        _headerGrid.PointerExited += OnHeaderPointerExited;
        _headerGrid.Tapped += OnHeaderTapped;

        Content = rootGrid;
    }

    public void BindViewModel(CategoryViewModel viewModel)
    {
        ViewModel = viewModel;

        _iconText.Text = string.IsNullOrEmpty(viewModel.IconGlyph)
            ? "" : viewModel.IconGlyph;
        _nameText.Text = viewModel.Name;

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CategoryViewModel.IsExpanded))
                UpdateExpandedState();
            if (args.PropertyName == nameof(CategoryViewModel.Name))
                _nameText.Text = viewModel.Name;
            if (args.PropertyName == nameof(CategoryViewModel.IconGlyph))
                _iconText.Text = string.IsNullOrEmpty(viewModel.IconGlyph)
                    ? "" : viewModel.IconGlyph;
        };

        viewModel.Resources.CollectionChanged += (_, _) => RefreshResources();

        UpdateExpandedState();
        RefreshResources();
    }

    private void UpdateExpandedState()
    {
        _expandArea.Visibility = ViewModel.IsExpanded
            ? Visibility.Visible : Visibility.Collapsed;
        _expandIndicator.Text = ViewModel.IsExpanded
            ? "" : "";
        _headerGrid.Background = ViewModel.IsExpanded
            ? ActiveBrush : RestBrush;
    }

    private void RefreshResources()
    {
        _resourcesPanel.Children.Clear();
        foreach (var resourceVm in ViewModel.Resources)
        {
            var tile = new ResourceTile();
            tile.BindViewModel(resourceVm);
            _resourcesPanel.Children.Add(tile);
        }
    }

    private void OnHeaderPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsExpanded)
            _headerGrid.Background = HoverBrush;
    }

    private void OnHeaderPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsExpanded)
            _headerGrid.Background = RestBrush;
    }

    private void OnHeaderTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.ToggleExpandCommand.Execute(null);
    }
}
