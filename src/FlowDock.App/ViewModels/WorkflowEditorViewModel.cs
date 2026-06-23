namespace FlowDock.App.ViewModels;

public partial class WorkflowEditorViewModel : ObservableObject
{
    private readonly IDataRepository _dataRepository;
    private readonly IWorkflowEngine _workflowEngine;

    [ObservableProperty]
    private string _workflowName = string.Empty;

    [ObservableProperty]
    private string _workflowDescription = string.Empty;

    [ObservableProperty]
    private WorkflowStepViewModel? _selectedStep;

    [ObservableProperty]
    private bool _isEditing;

    public ObservableCollection<WorkflowStepViewModel> Steps { get; } = new();

    public bool HasNoSteps => Steps.Count == 0;

    public WorkflowEditorViewModel(
        IDataRepository dataRepository,
        IWorkflowEngine workflowEngine)
    {
        _dataRepository = dataRepository;
        _workflowEngine = workflowEngine;
    }

    [RelayCommand]
    private void AddStep()
    {
        var step = new WorkflowStepViewModel
        {
            StepNumber = Steps.Count + 1,
        };
        Steps.Add(step);
        OnPropertyChanged(nameof(HasNoSteps));
    }

    [RelayCommand]
    private void RemoveStep()
    {
        if (SelectedStep is null)
            return;

        Steps.Remove(SelectedStep);
        SelectedStep = null;
        RenumberSteps();
        OnPropertyChanged(nameof(HasNoSteps));
    }

    [RelayCommand]
    private void MoveStepUp()
    {
        if (SelectedStep is null)
            return;

        var index = Steps.IndexOf(SelectedStep);
        if (index > 0)
        {
            Steps.Move(index, index - 1);
            RenumberSteps();
        }
    }

    [RelayCommand]
    private void MoveStepDown()
    {
        if (SelectedStep is null)
            return;

        var index = Steps.IndexOf(SelectedStep);
        if (index < Steps.Count - 1)
        {
            Steps.Move(index, index + 1);
            RenumberSteps();
        }
    }

    [RelayCommand]
    private async Task SaveWorkflow()
    {
        var workflow = BuildWorkflowDefinition();
        await _dataRepository.Workflows.AddAsync(workflow);
        await _dataRepository.SaveAllAsync();
    }

    [RelayCommand]
    private async Task TestRun()
    {
        var workflow = BuildWorkflowDefinition();
        await _workflowEngine.RunWorkflowAsync(workflow.Id);
    }

    public async Task SaveWorkflowAsync()
    {
        await SaveWorkflow();
    }

    public async Task TestRunWorkflowAsync()
    {
        await TestRun();
    }

    public async Task LoadWorkflowAsync(WorkflowDefinition workflow)
    {
        WorkflowName = workflow.Name;
        WorkflowDescription = workflow.Description;
        IsEditing = true;

        Steps.Clear();
        foreach (var step in workflow.Steps.OrderBy(s => s.StepNumber))
        {
            var stepVm = new WorkflowStepViewModel
            {
                StepNumber = step.StepNumber,
                ResourceId = step.ResourceId,
                WaitForWindow = step.WaitForWindow,
                WindowTitlePattern = step.WindowTitlePattern,
                WindowClassName = step.WindowClassName,
                WaitTimeoutMs = step.WaitTimeoutMs,
                IsEnabled = step.IsEnabled,
            };
            Steps.Add(stepVm);
        }
        OnPropertyChanged(nameof(HasNoSteps));
    }

    public async Task LoadAvailableResourcesAsync()
    {
        // Pre-load available resources for all steps so the dropdown pickers have data
        foreach (var step in Steps)
        {
            await step.LoadAvailableResourcesAsync(_dataRepository);
        }
    }

    private WorkflowDefinition BuildWorkflowDefinition()
    {
        return new WorkflowDefinition
        {
            Name = WorkflowName,
            Description = WorkflowDescription,
            Steps = Steps.Select(s => new WorkflowStep
            {
                StepNumber = s.StepNumber,
                ResourceId = s.ResourceId,
                WaitForWindow = s.WaitForWindow,
                WindowTitlePattern = s.WindowTitlePattern,
                WindowClassName = s.WindowClassName,
                WaitTimeoutMs = s.WaitTimeoutMs,
                IsEnabled = s.IsEnabled,
            }).ToList(),
        };
    }

    private void RenumberSteps()
    {
        for (var i = 0; i < Steps.Count; i++)
        {
            Steps[i].StepNumber = i + 1;
        }
    }
}
