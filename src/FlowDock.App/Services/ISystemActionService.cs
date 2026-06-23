namespace FlowDock.App.Services;

using FlowDock.App.Models;

public interface ISystemActionService
{
    Task ExecuteAsync(SystemActionType action);
}
