using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;

public sealed partial class StudentTeacherDirectoryViewModel : ObservableObject
{
    private readonly ListAvailableTeachersUseCase _listTeachersUseCase;
    private readonly CreateStudentRegistrationRequestUseCase _createRequestUseCase;
    private readonly GetCurrentStudentProfileUseCase _getCurrentStudentProfileUseCase;
    private Guid _clientOperationId = Guid.NewGuid();
    private StudentProfile? _currentProfile;

    public StudentTeacherDirectoryViewModel(
        ListAvailableTeachersUseCase listTeachersUseCase,
        CreateStudentRegistrationRequestUseCase createRequestUseCase,
        GetCurrentStudentProfileUseCase getCurrentStudentProfileUseCase)
    {
        _listTeachersUseCase = listTeachersUseCase;
        _createRequestUseCase = createRequestUseCase;
        _getCurrentStudentProfileUseCase = getCurrentStudentProfileUseCase;
    }

    public ObservableCollection<AvailableTeacher> Teachers { get; } = [];
    public IReadOnlyList<PublicHalaqa> PublicHalaqas => SelectedTeacher?.PublicHalaqas ?? [];

    [ObservableProperty] private AvailableTeacher? _selectedTeacher;
    [ObservableProperty] private PublicHalaqa? _selectedHalaqa;
    [ObservableProperty] private string? _searchText;
    [ObservableProperty] private string? _teacherCode;
    [ObservableProperty] private string? _messageText;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _searchError;
    [ObservableProperty] private string? _messageTextError;
    [ObservableProperty] private string? _profileError;

    public string RequestTitle => SelectedTeacher is null
        ? "اختر معلماً لبدء طلب التسجيل"
        : $"طلب تسجيل موجّه إلى {SelectedTeacher.DisplayName}";

    public string ProfileReadiness => _currentProfile is null
        ? "لم تُحمّل بيانات ملفك بعد."
        : "ستُرسل بيانات ملفك المحدثة مع الطلب دون تخزين محلي.";

    public event EventHandler? BackRequested;

    public void Initialize()
    {
        Teachers.Clear();
        SelectedTeacher = null;
        SelectedHalaqa = null;
        SearchText = null;
        TeacherCode = null;
        MessageText = null;
        CurrentPage = 1;
        LastPage = 1;
        Total = 0;
        _currentProfile = null;
        _clientOperationId = Guid.NewGuid();
        ClearFeedback();
        OnPropertyChanged(nameof(ProfileReadiness));
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task SearchAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadNextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            await LoadPageAsync(CurrentPage + 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            await LoadPageAsync(CurrentPage - 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task RefreshProfileAsync()
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _getCurrentStudentProfileUseCase.ExecuteAsync();
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            _currentProfile = result.Value;
            Message = "حُمّلت بيانات ملفك لتجهيز طلب التسجيل.";
            OnPropertyChanged(nameof(ProfileReadiness));
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (SelectedTeacher is null)
        {
            SetLocalFailure("اختر معلماً أولاً قبل إرسال الطلب.");
            return;
        }
        if (!TryCreateCommand(out var command, out var error))
        {
            SetLocalFailure(error!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _createRequestUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            MessageText = null;
            _clientOperationId = Guid.NewGuid();
            Message = "تم تقديم طلب التسجيل. راجع حالته بعد أن يعالج الخادم الطلب.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSelectedTeacherChanged(AvailableTeacher? value)
    {
        SelectedHalaqa = null;
        OnPropertyChanged(nameof(PublicHalaqas));
        OnPropertyChanged(nameof(RequestTitle));
        SubmitCommand.NotifyCanExecuteChanged();
    }

    partial void OnMessageTextChanged(string? value) => SubmitCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy;
    private bool CanSubmit() => !IsBusy && SelectedTeacher is not null;
    private bool CanNavigateBack() => !IsBusy;

    private async Task LoadPageAsync(int page)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listTeachersUseCase.ExecuteAsync(TeacherCode, SearchText, page);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Teachers.Clear();
            foreach (var teacher in result.Value.Teachers)
            {
                Teachers.Add(teacher);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            SelectedTeacher = null;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool TryCreateCommand(out CreateStudentRegistrationRequestCommand? command, out string? error)
    {
        command = null;
        error = null;
        var profile = _currentProfile;
        if (profile is null)
        {
            error = "حدّث بيانات ملفك أولاً قبل إرسال الطلب.";
            ProfileError = "حمّل الملف أو أكمل بياناته التفصيلية أولاً.";
            return false;
        }

        var attendance = profile.AttendancePreferences ?? profile.FollowUpPlan?.AttendancePreferences;
        if (profile.BirthDate is null || string.IsNullOrWhiteSpace(profile.Country) ||
            string.IsNullOrWhiteSpace(profile.City) || string.IsNullOrWhiteSpace(profile.Phone) ||
            string.IsNullOrWhiteSpace(profile.PhoneZone) || attendance is null || profile.FollowUpPlan is null)
        {
            error = "يجب أن يتضمن ملفك تاريخ الميلاد والدولة والمدينة والهاتف وتفضيلات الحضور وخطة المتابعة قبل إرسال الطلب.";
            ProfileError = "أكمل الحقول المطلوبة في ملف الطالب ثم حدّث البيانات هنا.";
            return false;
        }

        var applicationProfile = new RegistrationApplicationProfile(
            ToRegistrationGender(profile.Gender),
            profile.BirthDate.Value,
            profile.Country,
            profile.City,
            profile.Residence,
            profile.Phone,
            profile.PhoneZone,
            profile.WhatsappPhone,
            profile.WhatsappZone,
            profile.MemorizationLevel,
            profile.ReviewLevel,
            null);
        var previous = profile.PreviousMemorization is { } previousValue
            ? new RegistrationPreviousMemorization(
                previousValue.MemorizationLevel,
                previousValue.ReviewLevel,
                previousValue.MemorizedJuzCount,
                previousValue.MemorizedSurahIds,
                previousValue.PreviousTeacherNotes,
                previousValue.StopReasons)
            : null;
        var registrationAttendance = new RegistrationAttendancePreferences(
            attendance.Timezone,
            attendance.WeeklySlots.Select(slot => new RegistrationWeeklyAvailabilitySlot(
                slot.DayOfWeek,
                slot.From,
                slot.To,
                slot.Preferred)).ToArray(),
            attendance.PreferredSessionDurationMinutes);
        var registrationPlan = new RegistrationFollowUpPlan(
            ToContractValue(profile.FollowUpPlan.Frequency),
            profile.FollowUpPlan.Details.Select(detail => new RegistrationPlanDetail(
                ToContractValue(detail.TaskType),
                ToContractValue(detail.Unit),
                detail.Amount,
                detail.Notes)).ToArray(),
            profile.FollowUpPlan.StartsOn,
            profile.FollowUpPlan.EndsOn);

        command = new CreateStudentRegistrationRequestCommand(
            SelectedTeacher!.TeacherCode,
            SelectedHalaqa?.Id,
            MessageText,
            applicationProfile,
            previous,
            registrationAttendance,
            registrationPlan,
            _clientOperationId);
        return true;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SearchCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        RefreshProfileCommand.NotifyCanExecuteChanged();
        SubmitCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        SearchError = null;
        MessageTextError = null;
        ProfileError = null;
    }

    private void SetLocalFailure(string message)
    {
        ClearFeedback();
        IsError = true;
        Message = message;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        if (error?.FieldErrors is { Count: > 0 } fieldErrors)
        {
            foreach (var fieldError in fieldErrors)
            {
                var fieldMessage = string.Join(" ", fieldError.Messages);
                if (fieldError.Field is "message")
                {
                    MessageTextError = fieldMessage;
                }
                else if (fieldError.Field is "code" or "search")
                {
                    SearchError = fieldMessage;
                }
                else if (fieldError.Field.StartsWith("profile", StringComparison.Ordinal) ||
                         fieldError.Field.StartsWith("attendance_preferences", StringComparison.Ordinal) ||
                         fieldError.Field.StartsWith("follow_up_plan", StringComparison.Ordinal))
                {
                    ProfileError = fieldMessage;
                }
            }
        }
        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }

    private static RegistrationGender ToRegistrationGender(StudentGender value) =>
        value == StudentGender.Male ? RegistrationGender.Male : RegistrationGender.Female;

    private static string ToContractValue(FollowUpFrequency value) => value switch
    {
        FollowUpFrequency.Daily => "daily",
        FollowUpFrequency.OnceAWeek => "onceAWeek",
        FollowUpFrequency.TwiceAWeek => "twiceAWeek",
        FollowUpFrequency.ThriceAWeek => "thriceAWeek",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static string ToContractValue(QuranTaskType value) => value switch
    {
        QuranTaskType.Memorization => "memorization",
        QuranTaskType.Review => "review",
        QuranTaskType.Recitation => "recitation",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static string ToContractValue(QuranPlanUnit value) => value switch
    {
        QuranPlanUnit.Juz => "juz",
        QuranPlanUnit.Hizb => "hizb",
        QuranPlanUnit.HalfHizb => "halfHizb",
        QuranPlanUnit.QuarterHizb => "quarterHizb",
        QuranPlanUnit.Page => "page",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
}
