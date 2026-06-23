namespace FlowDock.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDataRepository _dataRepository;
    private readonly IHotkeyService _hotkeyService;
    private readonly IDockService _dockService;
    private readonly IThemeService _themeService;
    private readonly IMessenger _messenger;

    private CancellationTokenSource? _collapseCts;

    [ObservableProperty]
    private bool _isDockExpanded;

    [ObservableProperty]
    private InteractionMode _currentMode;

    public ObservableCollection<CategoryViewModel> Categories { get; } = new();

    public DockContainerViewModel Dock { get; }

    public MainViewModel(
        IDataRepository dataRepository,
        IHotkeyService hotkeyService,
        IDockService dockService,
        IThemeService themeService,
        IMessenger messenger)
    {
        _dataRepository = dataRepository;
        _hotkeyService = hotkeyService;
        _dockService = dockService;
        _themeService = themeService;
        _messenger = messenger;
        Dock = new DockContainerViewModel();

        _hotkeyService.HotkeyPressed += (_, _) => HandleHotkeyPressed();
    }

    public async Task LoadDataAsync()
    {
        await _dataRepository.LoadAllAsync();

        var settings = await _dataRepository.Settings.GetSettingsAsync();
        CurrentMode = settings.InteractionMode;
        Dock.DockWidth = settings.DockWidth;
        Dock.DockCollapsedWidth = settings.DockCollapsedWidth;
        Dock.DockEdge = settings.DockEdge;
        Dock.AnimationDuration = settings.DockAnimationDurationMs;

        _themeService.ApplyTheme(settings);

        Categories.Clear();
        var categories = await _dataRepository.Categories.GetAllAsync();
        foreach (var category in categories.OrderBy(c => c.SortOrder))
        {
            var categoryVm = new CategoryViewModel(category, _dataRepository);
            await categoryVm.LoadResourcesAsync();
            Categories.Add(categoryVm);
        }
    }

    public async Task InitializeAsync(IntPtr hwnd)
    {
        await LoadDataAsync();

        var settings = await _dataRepository.Settings.GetSettingsAsync();
        await _dockService.InitializeAsync(hwnd, settings);
        await _hotkeyService.RegisterAsync(
            hwnd,
            (uint)settings.HotkeyModifiersCode,
            (uint)settings.HotkeyVirtualKey);
    }

    public async Task ToggleDockAsync()
    {
        if (IsDockExpanded)
        {
            Dock.CollapseCommand.Execute(null);
            IsDockExpanded = false;
        }
        else
        {
            Dock.ExpandCommand.Execute(null);
            IsDockExpanded = true;
        }

        await Task.CompletedTask;
    }

    public void ToggleCategoryExpansion(CategoryViewModel categoryVm)
    {
        categoryVm.ToggleExpandCommand.Execute(null);
    }

    public void HandleHotkeyPressed()
    {
        _ = ToggleDockAsync();
    }

    public void HandleMouseEnterEdge()
    {
        CancelCollapseDelay();

        if (CurrentMode == InteractionMode.PureHover)
        {
            Dock.ExpandCommand.Execute(null);
            IsDockExpanded = true;
        }
    }

    public void HandleMouseLeaveEdge()
    {
        if (CurrentMode != InteractionMode.PureHover)
            return;

        StartCollapseDelay();
    }

    private void StartCollapseDelay()
    {
        CancelCollapseDelay();
        _collapseCts = new CancellationTokenSource();
        var token = _collapseCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                {
                    _ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                    {
                        Dock.CollapseCommand.Execute(null);
                        IsDockExpanded = false;
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Collapse was cancelled
            }
        }, token);
    }

    private void CancelCollapseDelay()
    {
        if (_collapseCts is not null)
        {
            _collapseCts.Cancel();
            _collapseCts.Dispose();
            _collapseCts = null;
        }
    }
}
