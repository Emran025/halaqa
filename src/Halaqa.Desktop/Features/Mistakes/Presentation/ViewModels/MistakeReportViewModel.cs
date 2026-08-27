using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Halaqa.Desktop.Features.Mistakes.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Mistakes.Presentation.ViewModels;

public sealed partial class MistakeReportViewModel : ObservableObject
{
    private readonly QueueMistakeUseCase queueMistakeUseCase;

    public MistakeReportViewModel(QueueMistakeUseCase queueMistakeUseCase)
    {
        this.queueMistakeUseCase = queueMistakeUseCase;
    }

    public IReadOnlyList<MistakeType> MistakeTypeOptions { get; } = new[]
    {
        MistakeType.Memory,
        MistakeType.Grammar,
        MistakeType.Pronunciation,
        MistakeType.Timing
    };

    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private Guid _taskId;
    [ObservableProperty] private string _taskTitle = string.Empty;
    [ObservableProperty] private bool _canRecordMistakes;
    [ObservableProperty] private string _ayahId = string.Empty;
    [ObservableProperty] private string _pageNumber = string.Empty;
    [ObservableProperty] private string _wordIndex = string.Empty;
    [ObservableProperty] private MistakeType _selectedMistakeType = MistakeType.Memory;
    [ObservableProperty] private string? _note;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize(Guid sessionId, Guid taskId, string taskTitle, bool canRecordMistakes)
    {
        SessionId = sessionId;
        TaskId = taskId;
        TaskTitle = taskTitle;
        CanRecordMistakes = canRecordMistakes;
        AyahId = string.Empty;
        PageNumber = string.Empty;
        WordIndex = string.Empty;
        SelectedMistakeType = MistakeType.Memory;
        Note = null;
        ClearFeedback();
        if (!CanRecordMistakes)
        {
            SetLocalFailure("لا تملك صلاحية تسجيل خطأ لهذه المهمة.");
        }
        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!TryReadPosition(out var ayahId, out var pageNumber, out var wordIndex, out var error))
        {
            SetLocalFailure(error!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await queueMistakeUseCase.ExecuteAsync(
                SessionId,
                TaskId,
                ayahId,
                pageNumber,
                wordIndex,
                SelectedMistakeType,
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim());
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            ApplySynchronizationState(result.Value);
            if (result.Value.SyncState is MistakeSyncState.Synced or MistakeSyncState.Pending)
            {
                AyahId = string.Empty;
                PageNumber = string.Empty;
                WordIndex = string.Empty;
                Note = null;
            }
        }
        finally
        {
            IsBusy = false;
            SubmitCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool TryReadPosition(out int ayahId, out int? pageNumber, out int wordIndex, out string? error)
    {
        ayahId = 0;
        pageNumber = null;
        wordIndex = 0;
        error = null;
        if (!int.TryParse(AyahId, NumberStyles.Integer, CultureInfo.InvariantCulture, out ayahId) || ayahId is < 1 or > 6236)
        {
            error = "رقم الآية يجب أن يكون بين 1 و6236.";
            return false;
        }
        if (!string.IsNullOrWhiteSpace(PageNumber))
        {
            if (!int.TryParse(PageNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPage) || parsedPage is < 1 or > 604)
            {
                error = "رقم الصفحة يجب أن يكون بين 1 و604 أو يترك فارغاً.";
                return false;
            }
            pageNumber = parsedPage;
        }
        if (!int.TryParse(WordIndex, NumberStyles.Integer, CultureInfo.InvariantCulture, out wordIndex) || wordIndex < 1)
        {
            error = "فهرس الكلمة يجب أن يبدأ من 1.";
            return false;
        }

        return true;
    }

    private bool CanSubmit() => CanRecordMistakes && !IsBusy && SessionId != Guid.Empty && TaskId != Guid.Empty;

    private void ApplySynchronizationState(PendingMistakeOperation operation)
    {
        IsError = operation.SyncState is MistakeSyncState.Conflict or MistakeSyncState.Failed;
        Message = operation.SyncState switch
        {
            MistakeSyncState.Synced => "تم إرسال الخطأ إلى الخادم بنجاح.",
            MistakeSyncState.Pending => "حُفظ الخطأ محلياً وسيُعاد إرسالُه عند توفر الاتصال.",
            MistakeSyncState.Conflict => operation.FailureReason ?? "تعارضت مزامنة الخطأ؛ راجع العملية قبل إعادة المحاولة.",
            MistakeSyncState.Failed => operation.FailureReason ?? "تعذر قبول الخطأ من الخادم.",
            _ => "تم حفظ حالة العملية."
        };
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

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تسجيل الخطأ.";
    }

    partial void OnCanRecordMistakesChanged(bool value) => SubmitCommand.NotifyCanExecuteChanged();
}
