namespace FlowDock.App.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
    private readonly IDataRepository? _dataRepository;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _iconGlyph = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasColorHint))]
    [NotifyPropertyChangedFor(nameof(ColorBrush))]
    private string _colorHint = string.Empty;

    public bool HasColorHint => !string.IsNullOrEmpty(ColorHint);

    public SolidColorBrush ColorBrush
    {
        get
        {
            if (!string.IsNullOrEmpty(ColorHint) && ColorHint.StartsWith("#") && ColorHint.Length >= 7)
            {
                try
                {
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(
                        255,
                        byte.Parse(ColorHint[1..3], System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(ColorHint[3..5], System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(ColorHint[5..7], System.Globalization.NumberStyles.HexNumber)));
                }
                catch
                {
                    return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
    }

    [ObservableProperty]
    private int _sortOrder;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isHovered;

    public ObservableCollection<ResourceItemViewModel> Resources { get; } = new();

    public CategoryViewModel()
    {
    }

    public CategoryViewModel(Category category, IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
        Id = category.Id;
        Name = category.Name;
        IconGlyph = category.IconGlyph;
        ColorHint = category.ColorHint;
        SortOrder = category.SortOrder;
    }

    public async Task LoadResourcesAsync()
    {
        var resources = await _dataRepository.Resources.GetByCategoryAsync(Id);
        Resources.Clear();
        foreach (var resource in resources.OrderBy(r => r.SortOrder))
        {
            Resources.Add(new ResourceItemViewModel(resource));
        }
    }

    [RelayCommand]
    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }

    [RelayCommand]
    private void AddResource()
    {
        // Placeholder for P4 — will open resource creation dialog for this category
    }
}
