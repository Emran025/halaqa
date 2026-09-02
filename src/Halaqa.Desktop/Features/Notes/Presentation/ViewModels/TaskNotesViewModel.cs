using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Notes.Domain.Entities;
using Halaqa.Desktop.Features.Notes.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notes.Presentation.ViewModels;

public sealed partial class TaskNotesViewModel : ObservableObject
{
    private readonly ListTaskNotesUseCase listTaskNotesUseCase;
    private readonly CreateTaskNoteUseCase createTaskNoteUseCase;
    private readonly UpdateTaskNoteUseCase updateTaskNoteUseCase;
    private readonly DeleteTaskNoteUseCase deleteTaskNoteUseCase;

    public TaskNotesViewModel(
        ListTaskNotesUseCase listTaskNotesUseCase,
        CreateTaskNoteUseCase createTaskNoteUseCase,
        UpdateTaskNoteUseCase updateTaskNoteUseCase,
        DeleteTaskNoteUseCase deleteTaskNoteUseCase)
    {
        this.listTaskNotesUseCase = listTaskNotesUseCase;
        this.createTaskNoteUseCase = createTaskNoteUseCase;
        this.updateTaskNoteUseCase = updateTaskNoteUseCase;
        this.deleteTaskNoteUseCase = deleteTaskNoteUseCase;
    }

    public ObservableCollection<TaskNote> Notes { get; } = new();

    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private Guid _taskId;
    [ObservableProperty] private Guid _currentUserId;
    [ObservableProperty] private string _taskTitle = string.Empty;
    [ObservableProperty] private TaskNote? _selectedNote;
    [ObservableProperty] private string _newBody = string.Empty;
    [ObservableProperty] private string _editBody = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize(Guid sessionId, Guid taskId, string taskTitle, Guid currentUserId)
    {
        SessionId = sessionId;
        TaskId = taskId;
        TaskTitle = taskTitle;
        CurrentUserId = currentUserId;
        Notes.Clear();
        SelectedNote = null;
        NewBody = string.Empty;
        EditBody = string.Empty;
        ClearFeedback();
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await listTaskNotesUseCase.ExecuteAsync(SessionId, TaskId);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error, "تعذر تحميل ملاحظات المهمة.");
                return;
            }

            Notes.Clear();
            foreach (var note in result.Value.Notes)
            {
                Notes.Add(note);
            }
            SelectedNote = Notes.FirstOrDefault(note => note.Author.Id == CurrentUserId);
            if (Notes.Count == 0)
            {
                Message = "لا توجد ملاحظات لهذه المهمة.";
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task CreateAsync()
    {
        var result = await ExecuteBusyAsync(
            () => createTaskNoteUseCase.ExecuteAsync(new CreateTaskNoteCommand(SessionId, TaskId, NewBody, Guid.NewGuid())),
            "تعذر إضافة الملاحظة.");
        if (result)
        {
            NewBody = string.Empty;
            await LoadAsync();
            if (!IsError)
            {
                Message = "تمت إضافة الملاحظة من الخادم.";
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task UpdateAsync()
    {
        var note = SelectedNote;
        if (note is null)
        {
            return;
        }

        var result = await ExecuteBusyAsync(
            () => updateTaskNoteUseCase.ExecuteAsync(new UpdateTaskNoteCommand(SessionId, TaskId, note.Id, EditBody)),
            "تعذر تحديث الملاحظة.");
        if (result)
        {
            await LoadAsync();
            if (!IsError)
            {
                Message = "تم تحديث الملاحظة من الخادم.";
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task DeleteAsync()
    {
        var note = SelectedNote;
        if (note is null)
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await deleteTaskNoteUseCase.ExecuteAsync(new DeleteTaskNoteCommand(SessionId, TaskId, note.Id));
            if (!result.IsSuccess)
            {
                SetFailure(result.Error, "تعذر حذف الملاحظة.");
                return;
            }

            await LoadAsync();
            if (!IsError)
            {
                Message = "تم حذف الملاحظة من الخادم.";
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private async Task<bool> ExecuteBusyAsync(Func<Task<Result<TaskNote>>> operation, string fallback)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await operation();
            if (!result.IsSuccess)
            {
                SetFailure(result.Error, fallback);
                return false;
            }
            return true;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool CanExecute() => !IsBusy && SessionId != Guid.Empty && TaskId != Guid.Empty && CurrentUserId != Guid.Empty;

    private bool CanEditSelected() => CanExecute() && SelectedNote?.Author.Id == CurrentUserId;

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetFailure(AppError? error, string fallback)
    {
        IsError = true;
        Message = error?.Message ?? fallback;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        CreateCommand.NotifyCanExecuteChanged();
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedNoteChanged(TaskNote? value)
    {
        EditBody = value?.Author.Id == CurrentUserId ? value.Body : string.Empty;
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }
}
