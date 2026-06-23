using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace FlowDock.App;

public sealed partial class MainWindow : Window
{
    private IntPtr _hwnd;
    private AppWindow? _appWindow;

    private readonly IDockService _dockService;
    private readonly MainViewModel _mainViewModel;
    private readonly DockContainer _dockContainer;

    public MainWindow(IDockService dockService, MainViewModel mainViewModel)
    {
        _dockService = dockService;
        _mainViewModel = mainViewModel;
        _dockContainer = new DockContainer();
    }

    public void BuildContent()
    {
        this.Content = _dockContainer;
    }

    public void SetupToolWindow()
    {
        _hwnd = WindowNative.GetWindowHandle(this);
        App.MainWindowHandle = _hwnd;

        _appWindow = AppWindow.GetFromWindowId(
            Win32Interop.GetWindowIdFromWindow(_hwnd));

        if (_appWindow != null)
        {
            _appWindow.IsShownInSwitchers = false;
            var presenter = _appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsAlwaysOnTop = true;
                presenter.IsResizable = false;
            }
        }

        int exStyle = NativeMethods.GetWindowLong(_hwnd, WindowStyles.GWL_EXSTYLE);
        exStyle |= WindowStyles.WS_EX_TOOLWINDOW;
        exStyle |= WindowStyles.WS_EX_NOACTIVATE;
        NativeMethods.SetWindowLong(_hwnd, WindowStyles.GWL_EXSTYLE, exStyle);

        var settings = Task.Run(async () =>
            await App.Host.Services.GetRequiredService<ISettingsRepository>().GetSettingsAsync()
        ).Result;

        _dockService.InitializeAsync(_hwnd, settings).Wait();
        _dockService.PositionDock(_hwnd, expanded: false);
    }

    public void HandleHotkeyMessage()
    {
        _mainViewModel.HandleHotkeyPressed();
    }
}
