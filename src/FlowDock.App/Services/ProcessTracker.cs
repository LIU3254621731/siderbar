namespace FlowDock.App.Services;

using FlowDock.App.Helpers.Win32;
using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class ProcessTracker : IProcessTracker, IDisposable
{
    private readonly ConcurrentDictionary<int, TrackedProcess> _processes = new();
    private readonly ILogger<ProcessTracker> _logger;
    private bool _disposed;

    public ProcessTracker(ILogger<ProcessTracker> logger)
    {
        _logger = logger;
    }

    public event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    public TrackedProcess RegisterProcess(Process process, string resourceId, string workflowId)
    {
        if (process is null)
            throw new ArgumentNullException(nameof(process));

        int pid = process.Id;

        var tracked = new TrackedProcess
        {
            ProcessId = pid,
            ProcessName = process.ProcessName,
            ResourceId = resourceId,
            WorkflowId = workflowId,
            StartedAt = DateTime.UtcNow,
            MainWindowHandle = process.MainWindowHandle
        };

        if (!_processes.TryAdd(pid, tracked))
        {
            _logger.LogWarning("Process {Pid} is already being tracked, replacing", pid);
            _processes[pid] = tracked;
        }

        process.EnableRaisingEvents = true;
        process.Exited += OnProcessExited;

        _logger.LogDebug(
            "Registered process: pid={Pid}, name='{Name}', resource='{ResourceId}', workflow='{WorkflowId}'",
            pid, process.ProcessName, resourceId, workflowId);

        return tracked;
    }

    public void UnregisterProcess(int processId)
    {
        if (_processes.TryRemove(processId, out var tracked))
        {
            _logger.LogDebug(
                "Unregistered process: pid={Pid}, name='{Name}'",
                processId, tracked.ProcessName);
        }
    }

    public IReadOnlyList<TrackedProcess> GetProcessesForWorkflow(string workflowId)
    {
        return _processes.Values
            .Where(p => p.WorkflowId == workflowId)
            .ToList()
            .AsReadOnly();
    }

    public async Task CloseAllForWorkflowAsync(string workflowId)
    {
        var processes = GetProcessesForWorkflow(workflowId);

        if (processes.Count == 0)
        {
            _logger.LogDebug("No tracked processes to close for workflow '{WorkflowId}'", workflowId);
            return;
        }

        _logger.LogInformation(
            "Closing {Count} process(es) for workflow '{WorkflowId}'",
            processes.Count, workflowId);

        // Phase 1: Send WM_CLOSE to each process's main window
        foreach (var tracked in processes)
        {
            try
            {
                if (tracked.MainWindowHandle != IntPtr.Zero)
                {
                    NativeMethods.SendMessageTimeout(
                        tracked.MainWindowHandle,
                        WindowStyles.WM_CLOSE,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        WindowStyles.SMTO_ABORTIFHUNG,
                        2000,
                        out _);

                    _logger.LogDebug("Sent WM_CLOSE to pid={Pid}, hwnd=0x{Handle:X8}",
                        tracked.ProcessId, tracked.MainWindowHandle);
                }
                else
                {
                    _logger.LogDebug("No main window handle for pid={Pid}", tracked.ProcessId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error sending WM_CLOSE to pid={Pid}", tracked.ProcessId);
            }
        }

        // Phase 2: Wait 3 seconds for graceful shutdown
        await Task.Delay(3000);

        // Phase 3: Kill survivors
        foreach (var tracked in processes)
        {
            try
            {
                using var proc = Process.GetProcessById(tracked.ProcessId);
                if (!proc.HasExited)
                {
                    _logger.LogWarning("Process pid={Pid} did not exit, killing", tracked.ProcessId);
                    proc.Kill(entireProcessTree: true);
                }
            }
            catch (ArgumentException)
            {
                _logger.LogDebug("Process pid={Pid} already exited", tracked.ProcessId);
            }
            catch (InvalidOperationException)
            {
                _logger.LogDebug("Process pid={Pid} already exited", tracked.ProcessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error killing process pid={Pid}", tracked.ProcessId);
            }

            UnregisterProcess(tracked.ProcessId);
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (sender is Process proc)
        {
            int pid = proc.Id;
            string name;

            try
            {
                name = proc.ProcessName;
            }
            catch
            {
                name = "<unknown>";
            }

            _logger.LogDebug("Process exited: pid={Pid}, name='{Name}'", pid, name);

            UnregisterProcess(pid);
            ProcessExited?.Invoke(this, new ProcessExitedEventArgs(pid, name));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (pid, _) in _processes)
        {
            try
            {
                using var proc = Process.GetProcessById(pid);
                proc.Exited -= OnProcessExited;
            }
            catch
            {
                // Process may already be gone
            }
        }

        _processes.Clear();
    }
}
