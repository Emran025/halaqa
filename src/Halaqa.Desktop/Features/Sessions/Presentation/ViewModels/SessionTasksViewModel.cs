using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

public sealed partial class SessionTasksViewModel : ObservableObject
{
    private readonly ListSessionTasksUseCase listSessionTasksUseCase;
    private readonly CreateSessionTaskUseCase createSessionTaskUseCase;
    private readonly UpdateSessionTaskUseCase updateSessionTaskUseCase;
    private readonly SaveSessionTaskDraftUseCase saveSessionTaskDraftUseCase;

    public SessionTasksViewModel(
        ListSessionTasksUseCase listSessionTasksUseCase,
        CreateSessionTaskUseCase createSessionTaskUseCase,
        UpdateSessionTaskUseCase updateSessionTaskUseCase,
        SaveSessionTaskDraftUseCase saveSessionTaskDraftUseCase)
    {
        this.listSessionTasksUseCase = listSessionTasksUseCase;
        this.createSessionTaskUseCase = createSessionTaskUseCase;
        this.updateSessionTaskUseCase = updateSessionTaskUseCase;
        this.saveSessionTaskDraftUseCase = saveSessionTaskDraftUseCase;
    }

    public ObservableCollection<SessionTaskListItem> Tasks { get; } = new();
    public IReadOnlyList<LocalizedOption<SessionTaskType>> TaskTypeOptions { get; } = new[]
    {
        new LocalizedOption<SessionTaskType>(SessionTaskType.Memorization, "حفظ"),
        new LocalizedOption<SessionTaskType>(SessionTaskType.Review, "مراجعة"),
        new LocalizedOption<SessionTaskType>(SessionTaskType.Recitation, "تلاوة")
    };

    public IReadOnlyList<LocalizedOption<OfficialSessionTaskState>> TaskStateOptions { get; } = new[]
    {
        new LocalizedOption<OfficialSessionTaskState>(OfficialSessionTaskState.Draft, "مسودة"),
        new LocalizedOption<OfficialSessionTaskState>(OfficialSessionTaskState.InProgress, "قيد التنفيذ"),
        new LocalizedOption<OfficialSessionTaskState>(OfficialSessionTaskState.Completed, "مكتملة"),
        new LocalizedOption<OfficialSessionTaskState>(OfficialSessionTaskState.Skipped, "متجاوزة"),
        new LocalizedOption<OfficialSessionTaskState>(OfficialSessionTaskState.Cancelled, "ملغاة")
    };

    [ObservableProperty] private SessionTaskListItem? _selectedTask;
    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private string _sessionTitle = string.Empty;
    [ObservableProperty] private SessionTaskType _newTaskType = SessionTaskType.Memorization;
    [ObservableProperty] private string _newSequenceNo = string.Empty;
    [ObservableProperty] private string _newPlannedAmount = string.Empty;
    [ObservableProperty] private string _newPlannedFromUnitId = string.Empty;
    [ObservableProperty] private string _newPlannedToUnitId = string.Empty;
    [ObservableProperty] private string _newStartPage = string.Empty;
    [ObservableProperty] private string _newStartAyahId = string.Empty;
    [ObservableProperty] private string _newEndPage = string.Empty;
    [ObservableProperty] private string _newEndAyahId = string.Empty;
    [ObservableProperty] private string _updatePlannedAmount = string.Empty;
    [ObservableProperty] private string _updateActualAmount = string.Empty;
    [ObservableProperty] private string _updatePlannedFromUnitId = string.Empty;
    [ObservableProperty] private string _updatePlannedToUnitId = string.Empty;
    [ObservableProperty] private string _updateStartPage = string.Empty;
    [ObservableProperty] private string _updateStartAyahId = string.Empty;
    [ObservableProperty] private string _updateEndPage = string.Empty;
    [ObservableProperty] private string _updateEndAyahId = string.Empty;
    [ObservableProperty] private string _updateCurrentPage = string.Empty;
    [ObservableProperty] private string _updateCurrentAyahId = string.Empty;
    [ObservableProperty] private OfficialSessionTaskState? _selectedUpdateState;
    [ObservableProperty] private string _draftCurrentPage = string.Empty;
    [ObservableProperty] private string _draftCurrentAyahId = string.Empty;
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
    public event EventHandler<SessionTaskListItem>? EvaluationRequested;
    public event EventHandler<SessionTaskListItem>? NotesRequested;

    public void Initialize(SessionListItem session, bool canCreateTasks, bool canReportMistakes)
    {
        SessionId = session.Id;
        SessionTitle = $"{session.TaskType} — {session.Teacher.Name} / {session.Student.Name}";
        CanCreateTasks = canCreateTasks;
        CanReportMistakes = canReportMistakes;
        NewTaskType = SessionTaskType.Memorization;
        ClearCreateInputs();
        ClearUpdateInputs();
        ClearDraftInputs();
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
        if (!TryCreateCommand(out var command, out var inputError))
        {
            SetLocalFailure(inputError!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await createSessionTaskUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error, "تعذر إنشاء مهمة الجلسة.");
                return;
            }

            ClearCreateInputs();
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

    [RelayCommand(CanExecute = nameof(CanUpdateTask))]
    private async Task UpdateTaskAsync()
    {
        if (!TryUpdateCommand(out var command, out var inputError))
        {
            SetLocalFailure(inputError!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await updateSessionTaskUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error, "تعذر تحديث مهمة الجلسة.");
                return;
            }

            ClearUpdateInputs();
            await LoadTasksAsync(keepBusy: true);
            if (!IsError)
            {
                Message = "تم تحديث المهمة من الخادم.";
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveDraft))]
    private async Task SaveDraftAsync()
    {
        if (!TrySaveDraftCommand(out var command, out var inputError))
        {
            SetLocalFailure(inputError!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await saveSessionTaskDraftUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error, "تعذر حفظ مسودة المهمة.");
                return;
            }

            ClearDraftInputs();
            await LoadTasksAsync(keepBusy: true);
            if (!IsError)
            {
                Message = "تم حفظ مسودة المهمة من الخادم.";
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenNotes))]
    private void OpenNotes()
    {
        if (SelectedTask is not null)
        {
            NotesRequested?.Invoke(this, SelectedTask);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEvaluate))]
    private void OpenEvaluation()
    {
        if (SelectedTask is not null)
        {
            EvaluationRequested?.Invoke(this, SelectedTask);
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
                SetFailure(result.Error, "تعذر تحميل مهام الجلسة.");
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

    private bool TryCreateCommand(out CreateSessionTaskCommand? command, out string? error)
    {
        command = null;
        if (!TryReadOptionalInt(NewSequenceNo, "رقم التسلسل", 1, null, out var sequenceNo, out error) ||
            !TryReadOptionalDecimal(NewPlannedAmount, "الكمية المخططة", out var plannedAmount, out error) ||
            !TryReadOptionalInt(NewPlannedFromUnitId, "معرّف وحدة بداية النطاق", 1, null, out var plannedFromUnitId, out error) ||
            !TryReadOptionalInt(NewPlannedToUnitId, "معرّف وحدة نهاية النطاق", 1, null, out var plannedToUnitId, out error) ||
            !TryReadOptionalInt(NewStartPage, "صفحة بداية النطاق", 1, 604, out var startPage, out error) ||
            !TryReadOptionalInt(NewStartAyahId, "آية بداية النطاق", 1, 6236, out var startAyahId, out error) ||
            !TryReadOptionalInt(NewEndPage, "صفحة نهاية النطاق", 1, 604, out var endPage, out error) ||
            !TryReadOptionalInt(NewEndAyahId, "آية نهاية النطاق", 1, 6236, out var endAyahId, out error))
        {
            return false;
        }

        command = new CreateSessionTaskCommand(
            SessionId,
            NewTaskType,
            Guid.NewGuid(),
            sequenceNo,
            plannedAmount,
            plannedFromUnitId,
            plannedToUnitId,
            startPage,
            startAyahId,
            endPage,
            endAyahId);
        return true;
    }

    private bool TrySaveDraftCommand(out SaveSessionTaskDraftCommand? command, out string? error)
    {
        command = null;
        error = null;
        if (SelectedTask is null)
        {
            error = "اختر مهمة لحفظ مسودتها أولاً.";
            return false;
        }
        if (!TryReadOptionalInt(DraftCurrentPage, "الصفحة الحالية", 1, 604, out var currentPage, out error) ||
            !TryReadOptionalInt(DraftCurrentAyahId, "الآية الحالية", 1, 6236, out var currentAyahId, out error))
        {
            return false;
        }

        command = new SaveSessionTaskDraftCommand(SessionId, SelectedTask.Id, Guid.NewGuid(), currentPage, currentAyahId);
        return true;
    }

    private bool TryUpdateCommand(out UpdateSessionTaskCommand? command, out string? error)
    {
        command = null;
        error = null;
        if (SelectedTask is null)
        {
            error = "اختر مهمة لتحديثها أولاً.";
            return false;
        }
        if (!TryReadOptionalDecimal(UpdatePlannedAmount, "الكمية المخططة", out var plannedAmount, out error) ||
            !TryReadOptionalDecimal(UpdateActualAmount, "الكمية الفعلية", out var actualAmount, out error) ||
            !TryReadOptionalInt(UpdatePlannedFromUnitId, "معرّف وحدة بداية النطاق", 1, null, out var plannedFromUnitId, out error) ||
            !TryReadOptionalInt(UpdatePlannedToUnitId, "معرّف وحدة نهاية النطاق", 1, null, out var plannedToUnitId, out error) ||
            !TryReadOptionalInt(UpdateStartPage, "صفحة بداية النطاق", 1, 604, out var startPage, out error) ||
            !TryReadOptionalInt(UpdateStartAyahId, "آية بداية النطاق", 1, 6236, out var startAyahId, out error) ||
            !TryReadOptionalInt(UpdateEndPage, "صفحة نهاية النطاق", 1, 604, out var endPage, out error) ||
            !TryReadOptionalInt(UpdateEndAyahId, "آية نهاية النطاق", 1, 6236, out var endAyahId, out error) ||
            !TryReadOptionalInt(UpdateCurrentPage, "الصفحة الحالية", 1, 604, out var currentPage, out error) ||
            !TryReadOptionalInt(UpdateCurrentAyahId, "الآية الحالية", 1, 6236, out var currentAyahId, out error))
        {
            return false;
        }

        var candidate = new UpdateSessionTaskCommand(
            SessionId,
            SelectedTask.Id,
            plannedFromUnitId,
            plannedToUnitId,
            startPage,
            startAyahId,
            endPage,
            endAyahId,
            currentPage,
            currentAyahId,
            SelectedUpdateState,
            plannedAmount,
            actualAmount);
        if (!candidate.HasChanges)
        {
            error = "أدخل حقلاً واحداً على الأقل لتحديث المهمة.";
            return false;
        }

        command = candidate;
        return true;
    }

    private static bool TryReadOptionalInt(string? value, string label, int min, int? max, out int? number, out string? error)
    {
        number = null;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed < min || (max.HasValue && parsed > max.Value))
        {
            error = max.HasValue
                ? $"{label} يجب أن يكون بين {min} و{max.Value}."
                : $"{label} يجب أن يبدأ من {min}.";
            return false;
        }

        number = parsed;
        return true;
    }

    private static bool TryReadOptionalDecimal(string? value, string label, out decimal? number, out string? error)
    {
        number = null;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) || parsed < 0)
        {
            error = $"{label} يجب أن تكون صفراً أو قيمة موجبة.";
            return false;
        }

        number = parsed;
        return true;
    }

    private void ClearCreateInputs()
    {
        NewSequenceNo = string.Empty;
        NewPlannedAmount = string.Empty;
        NewPlannedFromUnitId = string.Empty;
        NewPlannedToUnitId = string.Empty;
        NewStartPage = string.Empty;
        NewStartAyahId = string.Empty;
        NewEndPage = string.Empty;
        NewEndAyahId = string.Empty;
    }

    private void ClearDraftInputs()
    {
        DraftCurrentPage = string.Empty;
        DraftCurrentAyahId = string.Empty;
    }

    private void ClearUpdateInputs()
    {
        UpdatePlannedAmount = string.Empty;
        UpdateActualAmount = string.Empty;
        UpdatePlannedFromUnitId = string.Empty;
        UpdatePlannedToUnitId = string.Empty;
        UpdateStartPage = string.Empty;
        UpdateStartAyahId = string.Empty;
        UpdateEndPage = string.Empty;
        UpdateEndAyahId = string.Empty;
        UpdateCurrentPage = string.Empty;
        UpdateCurrentAyahId = string.Empty;
        SelectedUpdateState = null;
    }

    private bool CanLoad() => !IsBusy && SessionId != Guid.Empty;
    private bool CanCreateTask() => CanLoad() && CanCreateTasks;
    private bool CanUpdateTask() => CanLoad() && CanCreateTasks && SelectedTask is not null;
    private bool CanSaveDraft() => CanLoad() && CanReportMistakes && SelectedTask is not null;
    private bool CanOpenNotes() => CanLoad() && CanReportMistakes && SelectedTask is not null;
    private bool CanEvaluate() => CanLoad() && CanReportMistakes && SelectedTask is not null;
    private bool CanReportMistake() => CanLoad() && CanReportMistakes && SelectedTask is not null;

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
        CreateTaskCommand.NotifyCanExecuteChanged();
        UpdateTaskCommand.NotifyCanExecuteChanged();
        SaveDraftCommand.NotifyCanExecuteChanged();
        OpenNotesCommand.NotifyCanExecuteChanged();
        OpenEvaluationCommand.NotifyCanExecuteChanged();
        ReportMistakeCommand.NotifyCanExecuteChanged();
    }

    partial void OnCanCreateTasksChanged(bool value) => NotifyCommands();

    partial void OnCanReportMistakesChanged(bool value) => NotifyCommands();

    partial void OnSelectedTaskChanged(SessionTaskListItem? value)
    {
        ClearUpdateInputs();
        ClearDraftInputs();
        UpdateTaskCommand.NotifyCanExecuteChanged();
        SaveDraftCommand.NotifyCanExecuteChanged();
        OpenNotesCommand.NotifyCanExecuteChanged();
        OpenEvaluationCommand.NotifyCanExecuteChanged();
        ReportMistakeCommand.NotifyCanExecuteChanged();
    }
}
