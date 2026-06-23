using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;

namespace FlowDock.App;

public partial class App : Application
{
    public static IHost Host { get; private set; } = default!;
    public static IntPtr MainWindowHandle { get; set; }

    public App()
    {
        this.Resources.MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Host = CreateHostBuilder().Build();
        _ = Host.StartAsync();

        var mainWindow = Host.Services.GetRequiredService<MainWindow>();
        var mainViewModel = Host.Services.GetRequiredService<MainViewModel>();

        mainWindow.BuildContent();
        mainWindow.Activate();

        _ = mainViewModel.InitializeAsync(App.MainWindowHandle);
    }

    private static IHostBuilder CreateHostBuilder()
    {
        var logsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FlowDock", "logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logsDir, "flowdock-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureLogging(logging => logging.AddSerilog(Log.Logger))
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IDataRepository, JsonDataStore>();
                services.AddSingleton<IHotkeyService, HotkeyService>();
                services.AddSingleton<IDockService, DockService>();
                services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
                services.AddSingleton<IProcessTracker, ProcessTracker>();
                services.AddSingleton<IWindowDetectionService, WindowDetectionService>();
                services.AddSingleton<ISystemActionService, SystemActionService>();
                services.AddSingleton<IResourceLaunchService, ResourceLaunchService>();
                services.AddSingleton<IThemeService, ThemeService>();

                services.AddTransient<MainViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<WorkflowEditorViewModel>();
                services.AddTransient<DockContainerViewModel>();
                services.AddTransient<CategoryViewModel>();
                services.AddTransient<ResourcePanelViewModel>();
                services.AddTransient<ResourceItemViewModel>();

                services.AddSingleton<MainWindow>();
                services.AddSingleton<SettingsWindow>();
            });
    }
}
