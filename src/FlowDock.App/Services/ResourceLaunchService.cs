namespace FlowDock.App.Services;

using FlowDock.App.Models;
using Microsoft.Extensions.Logging;

public sealed class ResourceLaunchService : IResourceLaunchService
{
    private readonly ILogger<ResourceLaunchService> _logger;
    private readonly ISystemActionService _systemActionService;
    private readonly Lazy<IWorkflowEngine> _workflowEngine;

    // Lazy<IWorkflowEngine> breaks the circular DI chain between
    // ResourceLaunchService -> IWorkflowEngine -> IResourceLaunchService.
    public ResourceLaunchService(
        ILogger<ResourceLaunchService> logger,
        ISystemActionService systemActionService,
        Lazy<IWorkflowEngine> workflowEngine)
    {
        _logger = logger;
        _systemActionService = systemActionService;
        _workflowEngine = workflowEngine;
    }

    public async Task<Process?> LaunchAsync(ResourceItem resource)
    {
        _logger.LogInformation(
            "Launching resource '{Name}' (Id={Id}, Type={Type})",
            resource.Name, resource.Id, resource.Type);

        return resource.Type switch
        {
            ResourceType.Application => LaunchApplication(resource),
            ResourceType.Folder => LaunchFolder(resource),
            ResourceType.URL => LaunchUrl(resource),
            ResourceType.SystemAction => await LaunchSystemActionAsync(resource),
            ResourceType.Workflow => await LaunchWorkflowAsync(resource),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resource.Type), $"Unsupported resource type: {resource.Type}")
        };
    }

    private Process? LaunchApplication(ResourceItem resource)
    {
        if (string.IsNullOrWhiteSpace(resource.TargetPath))
        {
            _logger.LogWarning("Resource '{Name}' has empty TargetPath, skipping", resource.Name);
            return null;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = resource.TargetPath,
            Arguments = resource.Arguments ?? string.Empty,
            UseShellExecute = true
        };

        if (!string.IsNullOrWhiteSpace(resource.WorkingDirectory))
        {
            startInfo.WorkingDirectory = resource.WorkingDirectory;
        }

        if (resource.RunAsAdmin)
        {
            startInfo.Verb = "runas";
            _logger.LogDebug("Launching '{Name}' as administrator", resource.Name);
        }

        _logger.LogDebug(
            "Starting process: {Path} {Args}",
            startInfo.FileName, startInfo.Arguments);

        return Process.Start(startInfo);
    }

    private static Process? LaunchFolder(ResourceItem resource)
    {
        if (string.IsNullOrWhiteSpace(resource.TargetPath))
        {
            return null;
        }

        return Process.Start("explorer.exe", $"\"{resource.TargetPath}\"");
    }

    private static Process? LaunchUrl(ResourceItem resource)
    {
        if (string.IsNullOrWhiteSpace(resource.TargetPath))
        {
            return null;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = resource.TargetPath,
            UseShellExecute = true
        };

        return Process.Start(startInfo);
    }

    private async Task<Process?> LaunchSystemActionAsync(ResourceItem resource)
    {
        if (resource.SystemAction is null)
        {
            _logger.LogWarning("Resource '{Name}' is SystemAction but SystemAction is null", resource.Name);
            return null;
        }

        await _systemActionService.ExecuteAsync(resource.SystemAction.Value);
        return null; // System actions don't produce a process to track
    }

    private async Task<Process?> LaunchWorkflowAsync(ResourceItem resource)
    {
        if (string.IsNullOrWhiteSpace(resource.WorkflowId))
        {
            _logger.LogWarning("Resource '{Name}' is Workflow but WorkflowId is empty", resource.Name);
            return null;
        }

        _logger.LogInformation(
            "Delegating to workflow engine for resource '{Name}', workflowId={WorkflowId}",
            resource.Name, resource.WorkflowId);

        var result = await _workflowEngine.Value.RunWorkflowAsync(resource.WorkflowId);
        if (!result.Success)
        {
            _logger.LogError(
                "Workflow '{WorkflowId}' failed: {Error}",
                resource.WorkflowId, result.ErrorMessage);
        }

        return null; // Workflows are tracked internally by the engine
    }
}
