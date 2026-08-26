using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Presentation.ViewModels;

public sealed partial class StudentWeeklySlotEditor : ObservableObject
{
    [ObservableProperty] private int _dayOfWeek;
    [ObservableProperty] private string _from = "18:00";
    [ObservableProperty] private string _to = "18:30";
    [ObservableProperty] private bool _preferred;

    public static StudentWeeklySlotEditor FromDomain(StudentWeeklyAvailabilitySlot value) => new()
    {
        DayOfWeek = value.DayOfWeek,
        From = value.From.ToString("HH:mm", CultureInfo.InvariantCulture),
        To = value.To.ToString("HH:mm", CultureInfo.InvariantCulture),
        Preferred = value.Preferred
    };

    public bool TryToDomain(out StudentWeeklyAvailabilitySlot? value)
    {
        value = null;
        if (DayOfWeek is < 0 or > 6 ||
            !TimeOnly.TryParseExact(From, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) ||
            !TimeOnly.TryParseExact(To, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to) ||
            from >= to)
        {
            return false;
        }

        value = new StudentWeeklyAvailabilitySlot(DayOfWeek, from, to, Preferred);
        return true;
    }
}

public sealed partial class StudentPlanDetailEditor : ObservableObject
{
    [ObservableProperty] private string _taskType = "memorization";
    [ObservableProperty] private string _unit = "page";
    [ObservableProperty] private string _amount = "1";
    [ObservableProperty] private string? _notes;

    public static StudentPlanDetailEditor FromDomain(StudentPlanDetail value) => new()
    {
        TaskType = ToContractValue(value.TaskType),
        Unit = ToContractValue(value.Unit),
        Amount = value.Amount.ToString(CultureInfo.InvariantCulture),
        Notes = value.Notes
    };

    public bool TryToDomain(out StudentPlanDetailDraft? value)
    {
        value = null;
        if (!Enum.TryParse<QuranTaskType>(TaskType, ignoreCase: true, out var taskType) ||
            !Enum.TryParse<QuranPlanUnit>(Unit, ignoreCase: true, out var unit) ||
            !decimal.TryParse(Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0 ||
            Notes?.Length > 500)
        {
            return false;
        }

        value = new StudentPlanDetailDraft(taskType, unit, amount, NormalizeOptional(Notes));
        return true;
    }

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed partial class StudentProfileViewModel : ObservableObject
{
    private readonly GetCurrentStudentProfileUseCase _getCurrentStudentProfileUseCase;
    private readonly UpdateCurrentStudentProfileUseCase _updateCurrentStudentProfileUseCase;
    private StudentProfile? _loadedProfile;

    public StudentProfileViewModel(
        GetCurrentStudentProfileUseCase getCurrentStudentProfileUseCase,
        UpdateCurrentStudentProfileUseCase updateCurrentStudentProfileUseCase)
    {
        _getCurrentStudentProfileUseCase = getCurrentStudentProfileUseCase;
        _updateCurrentStudentProfileUseCase = updateCurrentStudentProfileUseCase;
        WeeklySlots.CollectionChanged += OnEditorCollectionChanged;
        PlanDetails.CollectionChanged += OnEditorCollectionChanged;
    }

    public ObservableCollection<StudentWeeklySlotEditor> WeeklySlots { get; } = new();
    public ObservableCollection<StudentPlanDetailEditor> PlanDetails { get; } = new();
    public IReadOnlyList<string> GenderOptions { get; } = new[] { "male", "female" };
    public IReadOnlyList<string> FrequencyOptions { get; } = new[] { "daily", "onceAWeek", "twiceAWeek", "thriceAWeek" };
    public IReadOnlyList<string> TaskTypeOptions { get; } = new[] { "memorization", "review", "recitation" };
    public IReadOnlyList<string> PlanUnitOptions { get; } = new[] { "juz", "hizb", "halfHizb", "quarterHizb", "page" };
    public IReadOnlyList<int> WeekDays { get; } = new[] { 0, 1, 2, 3, 4, 5, 6 };

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string? _birthDate;
    [ObservableProperty] private string? _gender;
    [ObservableProperty] private string? _country;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _residence;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _phoneZone;
    [ObservableProperty] private string? _whatsappPhone;
    [ObservableProperty] private string? _whatsappZone;
    [ObservableProperty] private string? _memorizationLevel;
    [ObservableProperty] private string? _reviewLevel;
    [ObservableProperty] private bool _clearBirthDate;
    [ObservableProperty] private bool _clearGender;
    [ObservableProperty] private bool _clearCountry;
    [ObservableProperty] private bool _clearCity;
    [ObservableProperty] private bool _clearResidence;
    [ObservableProperty] private bool _clearPhone;
    [ObservableProperty] private bool _clearPhoneZone;
    [ObservableProperty] private bool _clearWhatsappPhone;
    [ObservableProperty] private bool _clearWhatsappZone;
    [ObservableProperty] private bool _clearMemorizationLevel;
    [ObservableProperty] private bool _clearReviewLevel;
    [ObservableProperty] private string? _previousMemorizationLevel;
    [ObservableProperty] private string? _previousReviewLevel;
    [ObservableProperty] private string? _memorizedJuzCount;
    [ObservableProperty] private string? _memorizedSurahIds;
    [ObservableProperty] private string? _previousTeacherNotes;
    [ObservableProperty] private string? _stopReasons;
    [ObservableProperty] private string? _timezone;
    [ObservableProperty] private string? _preferredSessionDurationMinutes;
    [ObservableProperty] private string _planFrequency = "onceAWeek";
    [ObservableProperty] private string? _planStartsOn;
    [ObservableProperty] private string? _planEndsOn;
    [ObservableProperty] private string? _bio;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _nameError;
    [ObservableProperty] private string? _birthDateError;
    [ObservableProperty] private string? _genderError;
    [ObservableProperty] private string? _countryError;
    [ObservableProperty] private string? _cityError;
    [ObservableProperty] private string? _residenceError;
    [ObservableProperty] private string? _phoneError;
    [ObservableProperty] private string? _phoneZoneError;
    [ObservableProperty] private string? _whatsappPhoneError;
    [ObservableProperty] private string? _whatsappZoneError;
    [ObservableProperty] private string? _memorizationLevelError;
    [ObservableProperty] private string? _reviewLevelError;
    [ObservableProperty] private string? _previousMemorizationError;
    [ObservableProperty] private string? _attendancePreferencesError;
    [ObservableProperty] private string? _followUpPlanError;
    [ObservableProperty] private string? _bioError;

    public event EventHandler? BackRequested;
    public event EventHandler<StudentProfile>? ProfileUpdated;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
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

            ApplyProfile(result.Value);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!TryCreateUpdateCommand(out var command, out var localError))
        {
            SetLocalFailure(localError!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _updateCurrentStudentProfileUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            ApplyProfile(result.Value);
            Bio = null;
            Message = "تم حفظ الملف التفصيلي للطالب.";
            ProfileUpdated?.Invoke(this, result.Value);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void AddWeeklySlot() => WeeklySlots.Add(new StudentWeeklySlotEditor());

    [RelayCommand]
    private void RemoveWeeklySlot(StudentWeeklySlotEditor? slot)
    {
        if (slot is not null)
        {
            WeeklySlots.Remove(slot);
        }
    }

    [RelayCommand]
    private void AddPlanDetail() => PlanDetails.Add(new StudentPlanDetailEditor());

    [RelayCommand]
    private void RemovePlanDetail(StudentPlanDetailEditor? detail)
    {
        if (detail is not null)
        {
            PlanDetails.Remove(detail);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(IsBusy))
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanLoad() => !IsBusy;
    private bool CanSave() => !IsBusy && TryCreateUpdateCommand(out _, out _);
    private bool CanNavigateBack() => !IsBusy;

    private void ApplyProfile(StudentProfile profile)
    {
        _loadedProfile = profile;
        Name = profile.Name;
        Email = profile.Email;
        BirthDate = profile.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        Gender = ToContractValue(profile.Gender);
        Country = profile.Country;
        City = profile.City;
        Residence = profile.Residence;
        Phone = profile.Phone;
        PhoneZone = profile.PhoneZone;
        WhatsappPhone = profile.WhatsappPhone;
        WhatsappZone = profile.WhatsappZone;
        MemorizationLevel = profile.MemorizationLevel;
        ReviewLevel = profile.ReviewLevel;
        ClearBirthDate = false;
        ClearGender = false;
        ClearCountry = false;
        ClearCity = false;
        ClearResidence = false;
        ClearPhone = false;
        ClearPhoneZone = false;
        ClearWhatsappPhone = false;
        ClearWhatsappZone = false;
        ClearMemorizationLevel = false;
        ClearReviewLevel = false;
        Bio = null;

        var previous = profile.PreviousMemorization;
        PreviousMemorizationLevel = previous?.MemorizationLevel;
        PreviousReviewLevel = previous?.ReviewLevel;
        MemorizedJuzCount = previous?.MemorizedJuzCount?.ToString(CultureInfo.InvariantCulture);
        MemorizedSurahIds = previous is null ? null : string.Join(", ", previous.MemorizedSurahIds);
        PreviousTeacherNotes = previous?.PreviousTeacherNotes;
        StopReasons = previous?.StopReasons;

        var attendance = profile.AttendancePreferences ?? profile.FollowUpPlan?.AttendancePreferences;
        Timezone = attendance?.Timezone;
        PreferredSessionDurationMinutes = attendance?.PreferredSessionDurationMinutes?.ToString(CultureInfo.InvariantCulture);
        ReplaceEditors(WeeklySlots, attendance?.WeeklySlots.Select(StudentWeeklySlotEditor.FromDomain) ?? Array.Empty<StudentWeeklySlotEditor>());

        var followUpPlan = profile.FollowUpPlan;
        PlanFrequency = followUpPlan is null ? "onceAWeek" : ToContractValue(followUpPlan.Frequency);
        PlanStartsOn = followUpPlan?.StartsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        PlanEndsOn = followUpPlan?.EndsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        ReplaceEditors(PlanDetails, followUpPlan?.Details.Select(StudentPlanDetailEditor.FromDomain) ?? Array.Empty<StudentPlanDetailEditor>());
    }

    private bool TryCreateUpdateCommand(out UpdateStudentProfileCommand? command, out string? error)
    {
        command = null;
        error = null;
        var loaded = _loadedProfile;
        if (loaded is null)
        {
            error = "حمّل الملف التفصيلي أولاً قبل الحفظ.";
            return false;
        }

        if (!TryReadDate(BirthDate, "تاريخ الميلاد", out var birthDate, out error) ||
            !TryReadDate(PlanStartsOn, "تاريخ بداية الخطة", out var planStartsOn, out error) ||
            !TryReadDate(PlanEndsOn, "تاريخ نهاية الخطة", out var planEndsOn, out error))
        {
            return false;
        }

        if (!TryCreatePreviousMemorization(loaded.PreviousMemorization, out var previous, out var previousChanged, out error) ||
            !TryCreateAttendancePreferences(loaded.AttendancePreferences ?? loaded.FollowUpPlan?.AttendancePreferences, out var attendance, out var attendanceChanged, out error) ||
            !TryCreateFollowUpPlan(loaded.FollowUpPlan, planStartsOn, planEndsOn, out var followUpPlan, out var followUpPlanChanged, out error))
        {
            return false;
        }

        var gender = TryReadGender(Gender);
        if (!ClearGender && !string.IsNullOrWhiteSpace(Gender) && gender is null)
        {
            error = "اختر الجنس بصيغة صحيحة.";
            return false;
        }

        command = new UpdateStudentProfileCommand(
            ChangedRequired(Name, loaded.Name),
            ChangedDate(birthDate, loaded.BirthDate, ClearBirthDate),
            ChangedValue(gender, loaded.Gender, ClearGender),
            ChangedOptional(Country, loaded.Country, ClearCountry),
            ChangedOptional(City, loaded.City, ClearCity),
            ChangedOptional(Residence, loaded.Residence, ClearResidence),
            ChangedOptional(Phone, loaded.Phone, ClearPhone),
            ChangedOptional(PhoneZone, loaded.PhoneZone, ClearPhoneZone),
            ChangedOptional(WhatsappPhone, loaded.WhatsappPhone, ClearWhatsappPhone),
            ChangedOptional(WhatsappZone, loaded.WhatsappZone, ClearWhatsappZone),
            ChangedOptional(MemorizationLevel, loaded.MemorizationLevel, ClearMemorizationLevel),
            ChangedOptional(ReviewLevel, loaded.ReviewLevel, ClearReviewLevel),
            previousChanged ? StudentProfileUpdateField<StudentPreviousMemorization>.Set(previous) : StudentProfileUpdateField<StudentPreviousMemorization>.Omit(),
            attendanceChanged ? StudentProfileUpdateField<StudentAttendancePreferences>.Set(attendance) : StudentProfileUpdateField<StudentAttendancePreferences>.Omit(),
            followUpPlanChanged ? StudentProfileUpdateField<StudentFollowUpPlanDraft>.Set(followUpPlan) : StudentProfileUpdateField<StudentFollowUpPlanDraft>.Omit(),
            !string.IsNullOrWhiteSpace(Bio) ? StudentProfileUpdateField<string>.Set(Bio.Trim()) : StudentProfileUpdateField<string>.Omit());

        return true;
    }

    private bool TryCreatePreviousMemorization(
        StudentPreviousMemorization? loaded,
        out StudentPreviousMemorization? value,
        out bool changed,
        out string? error)
    {
        value = null;
        changed = false;
        error = null;
        var anyInput = !string.IsNullOrWhiteSpace(PreviousMemorizationLevel) ||
                       !string.IsNullOrWhiteSpace(PreviousReviewLevel) ||
                       !string.IsNullOrWhiteSpace(MemorizedJuzCount) ||
                       !string.IsNullOrWhiteSpace(MemorizedSurahIds) ||
                       !string.IsNullOrWhiteSpace(PreviousTeacherNotes) ||
                       !string.IsNullOrWhiteSpace(StopReasons);
        if (loaded is null && !anyInput)
        {
            return true;
        }

        decimal? juzCount = null;
        if (!string.IsNullOrWhiteSpace(MemorizedJuzCount))
        {
            if (!decimal.TryParse(MemorizedJuzCount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedJuz) || parsedJuz is < 0 or > 30)
            {
                error = "أدخل عدد أجزاء محفوظة بين 0 و30.";
                return false;
            }
            juzCount = parsedJuz;
        }

        value = new StudentPreviousMemorization(
            NormalizeOptional(PreviousMemorizationLevel),
            NormalizeOptional(PreviousReviewLevel),
            juzCount,
            (MemorizedSurahIds ?? string.Empty).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
            loaded?.LastCompletedUnit,
            NormalizeOptional(PreviousTeacherNotes),
            NormalizeOptional(StopReasons));
        changed = !Equals(value, loaded);
        return true;
    }

    private bool TryCreateAttendancePreferences(
        StudentAttendancePreferences? loaded,
        out StudentAttendancePreferences? value,
        out bool changed,
        out string? error)
    {
        value = null;
        changed = false;
        error = null;
        var anyInput = !string.IsNullOrWhiteSpace(Timezone) ||
                       !string.IsNullOrWhiteSpace(PreferredSessionDurationMinutes) ||
                       WeeklySlots.Count > 0;
        if (loaded is null && !anyInput)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(Timezone))
        {
            error = "أدخل المنطقة الزمنية لتفضيلات الحضور.";
            return false;
        }

        int? duration = null;
        if (!string.IsNullOrWhiteSpace(PreferredSessionDurationMinutes))
        {
            if (!int.TryParse(PreferredSessionDurationMinutes, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedDuration) || parsedDuration is < 10 or > 180)
            {
                error = "مدة الجلسة المفضلة يجب أن تكون بين 10 و180 دقيقة.";
                return false;
            }
            duration = parsedDuration;
        }

        var slots = new List<StudentWeeklyAvailabilitySlot>();
        foreach (var slot in WeeklySlots)
        {
            if (!slot.TryToDomain(out var parsedSlot) || parsedSlot is null)
            {
                error = "راجع اليوم وأوقات تفضيلات الحضور.";
                return false;
            }
            slots.Add(parsedSlot);
        }

        if (slots.Count == 0)
        {
            error = "أضف موعد حضور أسبوعياً واحداً على الأقل.";
            return false;
        }

        value = new StudentAttendancePreferences(Timezone.Trim(), slots, duration);
        changed = !Equals(value, loaded);
        return true;
    }

    private bool TryCreateFollowUpPlan(
        StudentFollowUpPlan? loaded,
        DateOnly? startsOn,
        DateOnly? endsOn,
        out StudentFollowUpPlanDraft? value,
        out bool changed,
        out string? error)
    {
        value = null;
        changed = false;
        error = null;
        var anyInput = PlanDetails.Count > 0 || !string.IsNullOrWhiteSpace(PlanStartsOn) || !string.IsNullOrWhiteSpace(PlanEndsOn) ||
                       (loaded is null && !string.Equals(PlanFrequency, "onceAWeek", StringComparison.Ordinal));
        if (loaded is null && !anyInput)
        {
            return true;
        }

        if (!Enum.TryParse<FollowUpFrequency>(PlanFrequency, ignoreCase: true, out var frequency))
        {
            error = "اختر وتيرة خطة المتابعة بصورة صحيحة.";
            return false;
        }

        if (startsOn is { } start && endsOn is { } end && end < start)
        {
            error = "لا يمكن أن يسبق تاريخ نهاية الخطة تاريخ بدايتها.";
            return false;
        }

        var details = new List<StudentPlanDetailDraft>();
        foreach (var detail in PlanDetails)
        {
            if (!detail.TryToDomain(out var parsedDetail) || parsedDetail is null)
            {
                error = "راجع نوع ووحدة وكمية تفاصيل خطة المتابعة.";
                return false;
            }
            details.Add(parsedDetail);
        }

        if (details.Count == 0)
        {
            error = "أضف تفصيلاً واحداً على الأقل لخطة المتابعة.";
            return false;
        }

        value = new StudentFollowUpPlanDraft(frequency, details, startsOn, endsOn);
        changed = !MatchesLoadedPlan(value, loaded);
        return true;
    }

    private static bool MatchesLoadedPlan(StudentFollowUpPlanDraft candidate, StudentFollowUpPlan? loaded) =>
        loaded is not null &&
        candidate.Frequency == loaded.Frequency &&
        candidate.StartsOn == loaded.StartsOn &&
        candidate.EndsOn == loaded.EndsOn &&
        candidate.Details.SequenceEqual(loaded.Details.Select(detail => new StudentPlanDetailDraft(
            detail.TaskType,
            detail.Unit,
            detail.Amount,
            detail.Notes)));

    private static StudentProfileUpdateField<string> ChangedRequired(string current, string loaded) =>
        !string.Equals(current.Trim(), loaded, StringComparison.Ordinal)
            ? StudentProfileUpdateField<string>.Set(current.Trim())
            : StudentProfileUpdateField<string>.Omit();

    private static StudentProfileUpdateField<string> ChangedOptional(string? current, string? loaded, bool clear) =>
        clear
            ? StudentProfileUpdateField<string>.Set(null)
            : !string.Equals(NormalizeOptional(current), loaded, StringComparison.Ordinal)
                ? StudentProfileUpdateField<string>.Set(NormalizeOptional(current))
                : StudentProfileUpdateField<string>.Omit();

    private static StudentProfileUpdateField<DateOnly?> ChangedDate(DateOnly? current, DateOnly? loaded, bool clear) =>
        clear
            ? StudentProfileUpdateField<DateOnly?>.Set(null)
            : current != loaded
                ? StudentProfileUpdateField<DateOnly?>.Set(current)
                : StudentProfileUpdateField<DateOnly?>.Omit();

    private static StudentProfileUpdateField<StudentGender?> ChangedValue(StudentGender? current, StudentGender loaded, bool clear) =>
        clear
            ? StudentProfileUpdateField<StudentGender?>.Set(null)
            : current is { } value && value != loaded
                ? StudentProfileUpdateField<StudentGender?>.Set(value)
                : StudentProfileUpdateField<StudentGender?>.Omit();

    private static bool TryReadDate(string? text, string label, out DateOnly? date, out string? error)
    {
        date = null;
        error = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            error = $"أدخل {label} بصيغة YYYY-MM-DD.";
            return false;
        }

        date = parsed;
        return true;
    }

    private static StudentGender? TryReadGender(string? value) =>
        Enum.TryParse<StudentGender>(value, ignoreCase: true, out var parsed) ? parsed : null;

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ReplaceEditors<T>(ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    private void OnEditorCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (INotifyPropertyChanged item in e.OldItems)
            {
                item.PropertyChanged -= OnEditorPropertyChanged;
            }
        }
        if (e.NewItems is not null)
        {
            foreach (INotifyPropertyChanged item in e.NewItems)
            {
                item.PropertyChanged += OnEditorPropertyChanged;
            }
        }
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        SaveCommand.NotifyCanExecuteChanged();

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        NameError = null;
        BirthDateError = null;
        GenderError = null;
        CountryError = null;
        CityError = null;
        ResidenceError = null;
        PhoneError = null;
        PhoneZoneError = null;
        WhatsappPhoneError = null;
        WhatsappZoneError = null;
        MemorizationLevelError = null;
        ReviewLevelError = null;
        PreviousMemorizationError = null;
        AttendancePreferencesError = null;
        FollowUpPlanError = null;
        BioError = null;
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
                switch (fieldError.Field)
                {
                    case "name": NameError = fieldMessage; break;
                    case "birth_date": BirthDateError = fieldMessage; break;
                    case "gender": GenderError = fieldMessage; break;
                    case "country": CountryError = fieldMessage; break;
                    case "city": CityError = fieldMessage; break;
                    case "residence": ResidenceError = fieldMessage; break;
                    case "phone": PhoneError = fieldMessage; break;
                    case "phone_zone": PhoneZoneError = fieldMessage; break;
                    case "whatsapp_phone": WhatsappPhoneError = fieldMessage; break;
                    case "whatsapp_zone": WhatsappZoneError = fieldMessage; break;
                    case "memorization_level": MemorizationLevelError = fieldMessage; break;
                    case "review_level": ReviewLevelError = fieldMessage; break;
                    case "previous_memorization": PreviousMemorizationError = fieldMessage; break;
                    case "attendance_preferences": AttendancePreferencesError = fieldMessage; break;
                    case "follow_up_plan": FollowUpPlanError = fieldMessage; break;
                    case "bio": BioError = fieldMessage; break;
                }
            }
        }
        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }
}
