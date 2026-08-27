using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

public sealed partial class SessionTasksViewModel : ObservableObject
{
    private readonly ListSessionTasksUseCase listSessionTasksUseCase;
    private readonly CreateSessionTaskUseCase createSessionTaskUseCase;

    public SessionTasksViewModel(
        ListSessionTasksUseCase listSessionTasksUseCase,
        CreateSessionTaskUseCase createSessionTaskUseCase)
    {
        this.listSessionTasksUseCase = listSessionTasksUseCase;
        this.createSessionTaskUseCase = createSessionTaskUseCase;
    }

    public ObservableCollection<SessionTaskListItem> Tasks { get; } = new();
    public IReadOnlyList<SessionTaskType> TaskTypeOptions { get; } = new[]
    {
        SessionTaskType.Memorization,
        SessionTaskType.Review,
        SessionTaskType.Recitation
    };

    [ObservableProperty] private SessionTaskListItem? _selectedTask;
    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private string _sessionTitle = string.Empty;
    [ObservableProperty] private SessionTaskType _newTaskType = SessionTaskType.Memorization;
    [ObservableProperty] private bool _canCreateTasks;
    [ObservableProperty] private bool _canReportMistakes;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;
    public event EventHandler<SessionTaskListItem>? MistakeReportingRequested;

    public void Initialize(SessionListItem session, bool canCreateTasks, bool canReportMistakes)
    {
        SessionId = session.Id;
        SessionTitle = $"{session.TaskType} — {session.Teacher.Name} / {session.Student.Name}";
        CanCreateTasks = canCreateTasks;
        CanReportMistakes = canReportMistakes;
        NewTaskType = SessionTaskType.Memorization;
        Tasks.Clear();
        SelectedTask = null;
        CurrentPage = 1;
        LastPage = 1;
        Total = 0;
        ClearFeedback();
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadTasksAsync();

    [RelayCommand(CanExecute = nameof(CanCreateTask))]
    private async Task CreateTaskAsync()
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var command = new CreateSessionTaskCommand(SessionId, NewTaskType, Guid.NewGuid());
            var result = await createSessionTaskUseCase.ExecuteAsync(command);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            await LoadTasksAsync(keepBusy: true);
            if (!IsError)
            {
                Message = "تم إنشاء المهمة وتحديث القائمة من الخادم.";
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanReportMistake))]
    private void ReportMistake()
    {
        if (SelectedTask is not null)
        {
            MistakeReportingRequested?.Invoke(this, SelectedTask);
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private async Task LoadTasksAsync(bool keepBusy = false)
    {
        if (!keepBusy)
        {
            IsBusy = true;
            ClearFeedback();
        }
        try
        {
            var result = await listSessionTasksUseCase.ExecuteAsync(SessionId);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Tasks.Clear();
            foreach (var task in result.Value.Tasks)
            {
                Tasks.Add(task);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            SelectedTask = Tasks.FirstOrDefault();
            if (Tasks.Count == 0)
            {
                Message = "لا توجد مهام معلنة لهذه الجلسة.";
            }
        }
        finally
        {
            if (!keepBusy)
            {
                IsBusy = false;
                NotifyCommands();
            }
        }
    }

    private bool CanLoad() => !IsBusy && SessionId != Guid.Empty;
    private bool CanCreateTask() => CanLoad() && CanCreateTasks;
    private bool CanReportMistake() => CanLoad() && CanReportMistakes && SelectedTask is not null;

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر إنشاء مهمة الجلسة.";
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        CreateTaskCommand.NotifyCanExecuteChanged();
        ReportMistakeCommand.NotifyCanExecuteChanged();
    }

    partial void OnCanCreateTasksChanged(bool value) => NotifyCommands();

    partial void OnCanReportMistakesChanged(bool value) => NotifyCommands();

    partial void OnSelectedTaskChanged(SessionTaskListItem? value) => ReportMistakeCommand.NotifyCanExecuteChanged();
}
