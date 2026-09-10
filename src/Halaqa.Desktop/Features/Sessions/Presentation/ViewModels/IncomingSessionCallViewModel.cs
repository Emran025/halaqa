using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

/// <summary>
/// ViewModel للإشعار الفوري بجلسة تسميع واردة من المعلم.
/// يُعرض على شكل أوفرلاي فوق واجهة التطبيق كاملةً عند استدعاء المعلم للطالب.
/// </summary>
public sealed partial class IncomingSessionCallViewModel : ObservableObject
{
    private readonly AcceptLiveSessionUseCase _acceptLiveSessionUseCase;
    private readonly RejectLiveSessionUseCase _rejectLiveSessionUseCase;

    // بيانات الجلسة الواردة
    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private string _teacherName = string.Empty;
    [ObservableProperty] private string _taskType = string.Empty;
    [ObservableProperty] private string _halaqaName = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    /// <summary>يُطلَق عندما يقبل الطالب الجلسة — يحمل SessionListItem المحدَّث.</summary>
    public event EventHandler<SessionListItem>? SessionAccepted;

    /// <summary>يُطلَق عندما يرفض الطالب الجلسة أو يتجاهلها.</summary>
    public event EventHandler? SessionDismissed;

    public IncomingSessionCallViewModel(
        AcceptLiveSessionUseCase acceptLiveSessionUseCase,
        RejectLiveSessionUseCase rejectLiveSessionUseCase)
    {
        _acceptLiveSessionUseCase = acceptLiveSessionUseCase;
        _rejectLiveSessionUseCase = rejectLiveSessionUseCase;
    }

    public void Show(SessionListItem session)
    {
        SessionId    = session.Id;
        TeacherName  = session.Teacher.Name;
        TaskType     = session.TaskType.ToString() switch
        {
            "Memorization" => "حفظ",
            "Review"       => "مراجعة",
            "Recitation"   => "تلاوة",
            _              => session.TaskType.ToString()
        };
        HalaqaName   = string.Empty; // لا يوجد اسم حلقة مباشرةً في SessionListItem
        IsBusy       = false;
        ErrorMessage = null;
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task AcceptAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await _acceptLiveSessionUseCase.ExecuteAsync(SessionId);
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error?.Message ?? "تعذر قبول الجلسة، حاول مرة أخرى.";
                return;
            }
            SessionAccepted?.Invoke(this, result.Value);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task RejectAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _rejectLiveSessionUseCase.ExecuteAsync(SessionId);
        }
        finally
        {
            IsBusy = false;
            SessionDismissed?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void Dismiss() => SessionDismissed?.Invoke(this, EventArgs.Empty);

    private bool CanExecute() => !IsBusy;
}
