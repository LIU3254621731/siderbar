using Microsoft.UI.Xaml;

namespace FlowDock.App.Views;

public sealed partial class SettingsWindow : Window
{
    public SettingsViewModel ViewModel { get; }

    private readonly IDockService _dockService;
    private readonly IHotkeyService _hotkeyService;

    public SettingsWindow()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<SettingsViewModel>();
        _dockService = App.Host.Services.GetRequiredService<IDockService>();
        _hotkeyService = App.Host.Services.GetRequiredService<IHotkeyService>();
        RootScrollViewer.DataContext = ViewModel;

        this.Closed += OnClosed;
    }

    public async Task LoadSettingsAsync()
    {
        await ViewModel.LoadAsync();
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();

        // Apply changes to running services
        await _dockService.ApplySettingsAsync(ViewModel.GetSettings());
        await _hotkeyService.ApplySettingsAsync(ViewModel.GetSettings());

        this.Close();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        await ViewModel.ReloadAsync();
    }
}
