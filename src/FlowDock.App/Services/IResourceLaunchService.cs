namespace FlowDock.App.Services;

using FlowDock.App.Models;

public interface IResourceLaunchService
{
    Task<Process?> LaunchAsync(ResourceItem resource);
}
