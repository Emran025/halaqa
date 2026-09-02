using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Evaluations.Domain.Entities;
using Halaqa.Desktop.Features.Evaluations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Evaluations.Presentation.ViewModels;

public sealed partial class TaskEvaluationViewModel : ObservableObject
{
    private readonly GetTaskEvaluationsUseCase getTaskEvaluationsUseCase;
    private readonly UpsertTaskEvaluationUseCase upsertTaskEvaluationUseCase;

    public TaskEvaluationViewModel(
        GetTaskEvaluationsUseCase getTaskEvaluationsUseCase,
        UpsertTaskEvaluationUseCase upsertTaskEvaluationUseCase)
    {
        this.getTaskEvaluationsUseCase = getTaskEvaluationsUseCase;
        this.upsertTaskEvaluationUseCase = upsertTaskEvaluationUseCase;
    }

    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private Guid _taskId;
    [ObservableProperty] private string _taskTitle = string.Empty;
    [ObservableProperty] private TaskEvaluatorRole _currentEvaluatorRole;
    [ObservableProperty] private TaskEvaluation? _teacherEvaluation;
    [ObservableProperty] private TaskEvaluation? _studentEvaluation;
    [ObservableProperty] private string _score = string.Empty;
    [ObservableProperty] private string? _comment;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize(Guid sessionId, Guid taskId, string taskTitle, TaskEvaluatorRole currentEvaluatorRole)
    {
        SessionId = sessionId;
        TaskId = taskId;
        TaskTitle = taskTitle;
        CurrentEvaluatorRole = currentEvaluatorRole;
        TeacherEvaluation = null;
        StudentEvaluation = null;
        Score = string.Empty;
        Comment = null;
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
            var result = await getTaskEvaluationsUseCase.ExecuteAsync(SessionId, TaskId);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error, "تعذر تحميل تقييمات المهمة.");
                return;
            }

            ApplySummary(result.Value);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task SaveAsync()
    {
        if (!decimal.TryParse(Score, NumberStyles.Number, CultureInfo.InvariantCulture, out var score) || score is < 0 or > 100)
        {
            SetLocalFailure("الدرجة يجب أن تكون بين 0 و100.");
            return;
        }
        if (Comment?.Length > 2000)
        {
            SetLocalFailure("الملاحظة يجب ألا تتجاوز 2000 حرف.");
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await upsertTaskEvaluationUseCase.ExecuteAsync(
                new UpsertTaskEvaluationCommand(SessionId, TaskId, score, string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim()));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error, "تعذر حفظ تقييم المهمة.");
                return;
            }

            ApplySummary(result.Value);
            Message = "تم حفظ التقييم من الخادم.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanExecute() => !IsBusy && SessionId != Guid.Empty && TaskId != Guid.Empty;

    private void ApplySummary(TaskEvaluationSummary summary)
    {
        TeacherEvaluation = summary.Teacher;
        StudentEvaluation = summary.Student;
        var own = CurrentEvaluatorRole == TaskEvaluatorRole.Teacher ? TeacherEvaluation : StudentEvaluation;
        if (own is not null)
        {
            Score = own.Score.ToString(CultureInfo.InvariantCulture);
            Comment = own.Comment;
        }
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetLocalFailure(string message)
    {
        IsError = true;
        Message = message;
    }

    private void SetFailure(AppError? error, string fallback)
    {
        IsError = true;
        Message = error?.Message ?? fallback;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }
}
