using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FlowDock.App.Views;

public sealed partial class WorkflowEditor : UserControl
{
    public WorkflowEditorViewModel ViewModel { get; }

    private readonly IWorkflowEngine _workflowEngine;

    public WorkflowEditor()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<WorkflowEditorViewModel>();
        _workflowEngine = App.Host.Services.GetRequiredService<IWorkflowEngine>();
        this.DataContext = ViewModel;
    }

    public async Task LoadWorkflowAsync(WorkflowDefinition workflow)
    {
        await ViewModel.LoadWorkflowAsync(workflow);
    }

    private void OnAddStepClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.AddStepCommand.Execute(null);
    }

    private void OnRemoveStepClicked(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is WorkflowStepViewModel stepVm)
        {
            ViewModel.RemoveStepCommand.Execute(stepVm);
        }
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveWorkflowAsync();
    }

    private async void OnTestRunClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.TestRunWorkflowAsync();
    }
}
