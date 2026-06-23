namespace FlowDock.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsRepository _settingsRepository;
    private AppSettings? _originalSettings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedInteractionModeIndex))]
    private InteractionMode _selectedInteractionMode;

    [ObservableProperty]
    private string _hotkeyModifiers = "Control+Alt";

    [ObservableProperty]
    private string _hotkeyKey = "D";

    [ObservableProperty]
    private int _dockWidth = 320;

    [ObservableProperty]
    private int _dockCollapsedWidth = 48;

    [ObservableProperty]
    private string _dockEdge = "Left";

    [ObservableProperty]
    private bool _useAcrylic = true;

    [ObservableProperty]
    private bool _useFrostedGlass = true;

    [ObservableProperty]
    private int _blurIntensity = 80;

    [ObservableProperty]
    private bool _followSystemTheme = true;

    [ObservableProperty]
    private string _customAccentColor = string.Empty;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    public int SelectedInteractionModeIndex
    {
        get => AvailableModes.IndexOf(SelectedInteractionMode);
        set
        {
            if (value >= 0 && value < AvailableModes.Count)
                SelectedInteractionMode = AvailableModes[value];
        }
    }

    public List<string> AvailableDockEdges { get; } = ["Left", "Right"];

    public List<InteractionMode> AvailableModes { get; } =
    [
        InteractionMode.PureHover,
        InteractionMode.HotkeyHover,
        InteractionMode.ClickToggle,
    ];

    public SettingsViewModel(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task LoadAsync()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        _originalSettings = CloneSettings(settings);
        ApplyFrom(settings);
    }

    [RelayCommand]
    private async Task Save()
    {
        var settings = BuildSettings();
        await _settingsRepository.SaveSettingsAsync(settings);
        _originalSettings = CloneSettings(settings);
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_originalSettings is not null)
        {
            ApplyFrom(_originalSettings);
        }
    }

    [RelayCommand]
    private async Task Reload()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        _originalSettings = CloneSettings(settings);
        ApplyFrom(settings);
    }

    public AppSettings GetSettings()
    {
        return BuildSettings();
    }

    public async Task SaveAsync()
    {
        await Save();
    }

    public async Task ReloadAsync()
    {
        await Reload();
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        ApplyFrom(new AppSettings());
    }

    private void ApplyFrom(AppSettings settings)
    {
        SelectedInteractionMode = settings.InteractionMode;
        HotkeyModifiers = settings.HotkeyModifiers;
        HotkeyKey = settings.HotkeyKey;
        DockWidth = settings.DockWidth;
        DockCollapsedWidth = settings.DockCollapsedWidth;
        DockEdge = settings.DockEdge;
        UseAcrylic = settings.UseAcrylic;
        UseFrostedGlass = settings.UseFrostedGlass;
        BlurIntensity = settings.BlurIntensity;
        FollowSystemTheme = settings.FollowSystemTheme;
        CustomAccentColor = settings.CustomAccentColor;
        StartWithWindows = settings.StartWithWindows;
        MinimizeToTray = settings.MinimizeToTray;
    }

    private AppSettings BuildSettings()
    {
        return new AppSettings
        {
            InteractionMode = SelectedInteractionMode,
            HotkeyModifiers = HotkeyModifiers,
            HotkeyKey = HotkeyKey,
            DockWidth = DockWidth,
            DockCollapsedWidth = DockCollapsedWidth,
            DockEdge = DockEdge,
            UseAcrylic = UseAcrylic,
            UseFrostedGlass = UseFrostedGlass,
            BlurIntensity = BlurIntensity,
            FollowSystemTheme = FollowSystemTheme,
            CustomAccentColor = CustomAccentColor,
            StartWithWindows = StartWithWindows,
            MinimizeToTray = MinimizeToTray,
        };
    }

    private static AppSettings CloneSettings(AppSettings source)
    {
        return new AppSettings
        {
            InteractionMode = source.InteractionMode,
            HotkeyModifiers = source.HotkeyModifiers,
            HotkeyKey = source.HotkeyKey,
            HotkeyVirtualKey = source.HotkeyVirtualKey,
            HotkeyModifiersCode = source.HotkeyModifiersCode,
            DockWidth = source.DockWidth,
            DockCollapsedWidth = source.DockCollapsedWidth,
            DockAnimationDurationMs = source.DockAnimationDurationMs,
            DockEdge = source.DockEdge,
            UseAcrylic = source.UseAcrylic,
            UseFrostedGlass = source.UseFrostedGlass,
            BlurIntensity = source.BlurIntensity,
            FollowSystemTheme = source.FollowSystemTheme,
            CustomAccentColor = source.CustomAccentColor,
            TargetMonitorIndex = source.TargetMonitorIndex,
            StartWithWindows = source.StartWithWindows,
            MinimizeToTray = source.MinimizeToTray,
            ShowOnAllDesktops = source.ShowOnAllDesktops,
            UpdatedAt = source.UpdatedAt,
        };
    }
}
