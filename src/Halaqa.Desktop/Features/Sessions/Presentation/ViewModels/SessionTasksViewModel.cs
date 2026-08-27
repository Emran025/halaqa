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

    public SessionTasksViewModel(ListSessionTasksUseCase listSessionTasksUseCase)
    {
        this.listSessionTasksUseCase = listSessionTasksUseCase;
    }

    public ObservableCollection<SessionTaskListItem> Tasks { get; } = new();

    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private string _sessionTitle = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize(SessionListItem session)
    {
        SessionId = session.Id;
        SessionTitle = $"{session.TaskType} — {session.Teacher.Name} / {session.Student.Name}";
        Tasks.Clear();
        CurrentPage = 1;
        LastPage = 1;
        Total = 0;
        ClearFeedback();
        LoadCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearFeedback();
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
            if (Tasks.Count == 0)
            {
                Message = "لا توجد مهام معلنة لهذه الجلسة.";
            }
        }
        finally
        {
            IsBusy = false;
            LoadCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanLoad() => !IsBusy && SessionId != Guid.Empty;

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تحميل مهام الجلسة.";
    }
}
