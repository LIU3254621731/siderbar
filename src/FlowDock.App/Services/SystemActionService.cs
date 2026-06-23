namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class SystemActionService : ISystemActionService
{
    private const int SC_MONITORPOWER = 0xF170;
    private static readonly IntPtr HWND_BROADCAST = new(0xFFFF);

    private readonly ILogger<SystemActionService> _logger;

    public SystemActionService(ILogger<SystemActionService> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(SystemActionType action)
    {
        _logger.LogInformation("Executing system action: {Action}", action);

        switch (action)
        {
            case SystemActionType.LockWorkstation:
                LockWorkstation();
                break;

            case SystemActionType.Sleep:
                SetSuspendState(hibernate: false);
                break;

            case SystemActionType.Hibernate:
                SetSuspendState(hibernate: true);
                break;

            case SystemActionType.Shutdown:
                StartShutdownProcess("/s /t 5");
                break;

            case SystemActionType.Restart:
                StartShutdownProcess("/r /t 5");
                break;

            case SystemActionType.DisplayOff:
                TurnDisplayOff();
                break;

            case SystemActionType.EmptyRecycleBin:
                EmptyRecycleBinInternal();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(action), $"Unsupported system action: {action}");
        }

        return Task.CompletedTask;
    }

    private void LockWorkstation()
    {
        _logger.LogDebug("Locking workstation");
        if (!NativeMethods.LockWorkStation())
        {
            int error = Marshal.GetLastWin32Error();
            _logger.LogError("LockWorkStation failed with error {Error}", error);
            throw new InvalidOperationException(
                $"LockWorkStation failed. Win32 error: {error}");
        }
    }

    private void SetSuspendState(bool hibernate)
    {
        _logger.LogDebug("Setting suspend state: hibernate={Hibernate}", hibernate);
        if (!NativeMethods.SetSuspendState(hibernate, forceCritical: true, disableWakeEvent: true))
        {
            int error = Marshal.GetLastWin32Error();
            _logger.LogError("SetSuspendState failed with error {Error}", error);
            throw new InvalidOperationException(
                $"SetSuspendState failed. Win32 error: {error}");
        }
    }

    private void StartShutdownProcess(string args)
    {
        _logger.LogDebug("Starting shutdown with args: {Args}", args);
        Process.Start("shutdown", args);
    }

    private void TurnDisplayOff()
    {
        _logger.LogDebug("Turning display off");
        NativeMethods.SendMessageTimeout(
            HWND_BROADCAST,
            WindowStyles.WM_SYSCOMMAND,
            new IntPtr(SC_MONITORPOWER),
            new IntPtr(2),
            WindowStyles.SMTO_NORMAL,
            2000,
            out _);
    }

    private void EmptyRecycleBinInternal()
    {
        _logger.LogDebug("Emptying recycle bin");

        // Use SHEmptyRecycleBin via shell32
        int result = SHEmptyRecycleBin(IntPtr.Zero, null,
            SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

        if (result != 0)
        {
            _logger.LogWarning(
                "SHEmptyRecycleBin returned {Result}, trying fallback", result);

            // Fallback: delete $Recycle.Bin contents
            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    string recyclePath = Path.Combine(drive.RootDirectory.FullName, "$Recycle.Bin");
                    if (Directory.Exists(recyclePath))
                    {
                        var di = new DirectoryInfo(recyclePath);
                        foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
                        {
                            try { file.Delete(); } catch { /* ignore */ }
                        }
                        foreach (var dir in di.GetDirectories("*", SearchOption.AllDirectories))
                        {
                            try { dir.Delete(true); } catch { /* ignore */ }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback recycle bin empty failed");
            }
        }
    }

    // ─── P/Invoke for SHEmptyRecycleBin ─────────────────────────

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int SHEmptyRecycleBin(
        IntPtr hwnd,
        string? pszRootPath,
        uint dwFlags);
}
