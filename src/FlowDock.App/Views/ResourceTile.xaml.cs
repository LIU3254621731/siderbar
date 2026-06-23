namespace FlowDock.App.Views;

public sealed class ResourceTile : UserControl
{
    public ResourceItemViewModel ViewModel { get; private set; } = null!;

    private readonly Grid _rootGrid;
    private readonly TextBlock _iconText;
    private readonly TextBlock _nameText;

    private static readonly SolidColorBrush HoverBrush =
        new(Windows.UI.Color.FromArgb(20, 255, 255, 255));

    private static readonly SolidColorBrush TransparentBrush =
        new(Colors.Transparent);

    public ResourceTile()
    {
        _rootGrid = new Grid
        {
            Height = 36,
            Padding = new Thickness(12, 0, 8, 0),
            CornerRadius = new CornerRadius(4),
            Background = TransparentBrush
        };
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        _iconText = new TextBlock
        {
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = new FontFamily("Segoe Fluent Icons"),
            Text = ""
        };
        Grid.SetColumn(_iconText, 0);
        _rootGrid.Children.Add(_iconText);

        _nameText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            FontSize = 12,
            Foreground = new SolidColorBrush(
                Windows.UI.Color.FromArgb(220, 255, 255, 255))
        };
        Grid.SetColumn(_nameText, 1);
        _rootGrid.Children.Add(_nameText);

        _rootGrid.PointerEntered += (_, _) =>
            _rootGrid.Background = HoverBrush;
        _rootGrid.PointerExited += (_, _) =>
            _rootGrid.Background = TransparentBrush;
        _rootGrid.Tapped += (_, _) =>
            ViewModel?.LaunchCommand.Execute(null);

        Content = _rootGrid;
    }

    public void BindViewModel(ResourceItemViewModel viewModel)
    {
        ViewModel = viewModel;
        _iconText.Text = string.IsNullOrEmpty(viewModel.IconGlyph)
            ? "" : viewModel.IconGlyph;
        _nameText.Text = viewModel.Name;
    }
}
