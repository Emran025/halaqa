using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class StudentRecitationProfileViewModel : ObservableObject
{
    private readonly GetStudentProfileByIdUseCase? _getStudentProfileByIdUseCase;

    [ObservableProperty] private string _studentName = string.Empty;
    [ObservableProperty] private string _studentCode = string.Empty;
    [ObservableProperty] private string _halaqaName = string.Empty;
    [ObservableProperty] private int? _currentMemorizationPage;
    [ObservableProperty] private int? _currentReviewPage;
    [ObservableProperty] private int? _currentRecitationPage;
    [ObservableProperty] private string _lastEvaluation = string.Empty;
    [ObservableProperty] private int _totalMistakesRecorded;
    [ObservableProperty] private double _averageScore;
    [ObservableProperty] private int _totalSessions;
    [ObservableProperty] private string _attendanceFrom = string.Empty;
    [ObservableProperty] private string _attendanceTo = string.Empty;
    [ObservableProperty] private bool _hasMemorizationPlan;
    [ObservableProperty] private bool _hasReviewPlan;
    [ObservableProperty] private bool _hasRecitationPlan;

    // Personal & Registration Info
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _whatsappPhone = string.Empty;
    [ObservableProperty] private string _gender = string.Empty;
    [ObservableProperty] private string _birthDate = string.Empty;
    [ObservableProperty] private string _age = string.Empty;
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _residence = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private string _registrationStatus = string.Empty;
    [ObservableProperty] private string _memorizationLevel = string.Empty;
    [ObservableProperty] private string _reviewLevel = string.Empty;
    [ObservableProperty] private string _previousMemorizedJuzCount = string.Empty;
    [ObservableProperty] private string _previousMemorizedSurahs = string.Empty;
    [ObservableProperty] private string _previousTeacherNotes = string.Empty;
    [ObservableProperty] private string _stopReasons = string.Empty;
    [ObservableProperty] private string _timezone = string.Empty;
    [ObservableProperty] private string _preferredSessionDuration = string.Empty;
    [ObservableProperty] private string _weeklyAvailability = string.Empty;
    [ObservableProperty] private bool _isLoadingProfile;
    [ObservableProperty] private bool _hasLoadedProfile;
    [ObservableProperty] private string? _profileErrorMessage;

    private StudentFollowUpSummary? _student;

    public ObservableCollection<SessionReport> SessionHistory { get; } = new();

    public event EventHandler? BackRequested;
    public event EventHandler<(StudentFollowUpSummary Student, string TaskType, int TargetPage)>? RecitationRequested;

    public StudentRecitationProfileViewModel(GetStudentProfileByIdUseCase? getStudentProfileByIdUseCase = null)
    {
        _getStudentProfileByIdUseCase = getStudentProfileByIdUseCase;
    }

    public void Initialize(StudentFollowUpSummary student, IReadOnlyList<SessionReport> reports)
    {
        _student = student;
        StudentName = student.StudentName;
        StudentCode = student.StudentCode ?? string.Empty;
        HalaqaName = student.HalaqaName ?? string.Empty;
        CurrentMemorizationPage = student.CurrentMemorizationPage;
        CurrentReviewPage = student.CurrentReviewPage;
        CurrentRecitationPage = student.CurrentRecitationPage;
        LastEvaluation = student.LastEvaluation ?? string.Empty;
        TotalMistakesRecorded = student.TotalMistakesRecorded;
        AttendanceFrom = student.AttendanceFrom;
        AttendanceTo = student.AttendanceTo;
        HasMemorizationPlan = student.HasMemorizationPlan;
        HasReviewPlan = student.HasReviewPlan;
        HasRecitationPlan = student.HasRecitationPlan;

        SessionHistory.Clear();
        foreach (var r in reports.OrderByDescending(r => r.CompletedAt))
            SessionHistory.Add(r);

        RefreshStats();

        _ = LoadPersonalProfileAsync(student.StudentId);
    }

    [RelayCommand]
    public async Task RefreshPersonalProfileAsync()
    {
        if (_student != null)
        {
            await LoadPersonalProfileAsync(_student.StudentId);
        }
    }

    public async Task LoadPersonalProfileAsync(Guid studentId)
    {
        if (_getStudentProfileByIdUseCase == null)
            return;

        IsLoadingProfile = true;
        ProfileErrorMessage = null;
        try
        {
            var result = await _getStudentProfileByIdUseCase.ExecuteAsync(studentId);
            if (!result.IsSuccess || result.Value is null)
            {
                ProfileErrorMessage = result.Error?.Message ?? "تعذر جلب البيانات الشخصية للطالب.";
                return;
            }

            var profile = result.Value;
            Email = profile.Email ?? string.Empty;
            Phone = !string.IsNullOrWhiteSpace(profile.PhoneZone) ? $"{profile.PhoneZone} {profile.Phone}" : (profile.Phone ?? string.Empty);
            WhatsappPhone = !string.IsNullOrWhiteSpace(profile.WhatsappZone) ? $"{profile.WhatsappZone} {profile.WhatsappPhone}" : (profile.WhatsappPhone ?? string.Empty);
            Gender = profile.Gender == StudentGender.Male ? "ذكر" : "أنثى";
            BirthDate = profile.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
            Age = profile.BirthDate.HasValue
                ? $"{DateTime.Today.Year - profile.BirthDate.Value.Year} سنة"
                : string.Empty;
            Country = profile.Country ?? string.Empty;
            City = profile.City ?? string.Empty;
            Residence = profile.Residence ?? string.Empty;
            Location = string.Join(" - ", new[] { profile.Country, profile.City, profile.Residence }.Where(s => !string.IsNullOrWhiteSpace(s)));
            RegistrationStatus = profile.Status switch
            {
                "active" => "نشط",
                "pending" => "قيد الانتظار",
                _ => profile.Status ?? string.Empty
            };
            MemorizationLevel = profile.MemorizationLevel ?? "غير محدد";
            ReviewLevel = profile.ReviewLevel ?? "غير محدد";

            if (profile.PreviousMemorization != null)
            {
                var prev = profile.PreviousMemorization;
                PreviousMemorizedJuzCount = prev.MemorizedJuzCount.HasValue
                    ? $"{prev.MemorizedJuzCount.Value} جزء"
                    : "غير محدد";
                PreviousMemorizedSurahs = prev.MemorizedSurahIds.Count > 0
                    ? string.Join("، ", prev.MemorizedSurahIds)
                    : "لا يوجد";
                PreviousTeacherNotes = !string.IsNullOrWhiteSpace(prev.PreviousTeacherNotes)
                    ? prev.PreviousTeacherNotes
                    : "لا توجد ملاحظات سابقة";
                StopReasons = !string.IsNullOrWhiteSpace(prev.StopReasons)
                    ? prev.StopReasons
                    : "لا توجد";
            }
            else
            {
                PreviousMemorizedJuzCount = "غير محدد";
                PreviousMemorizedSurahs = "لا يوجد";
                PreviousTeacherNotes = "لا توجد";
                StopReasons = "لا توجد";
            }

            if (profile.AttendancePreferences != null)
            {
                var att = profile.AttendancePreferences;
                Timezone = att.Timezone ?? string.Empty;
                PreferredSessionDuration = att.PreferredSessionDurationMinutes.HasValue
                    ? $"{att.PreferredSessionDurationMinutes.Value} دقيقة"
                    : "غير محدد";

                if (att.WeeklySlots.Count > 0)
                {
                    var days = new[] { "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };
                    WeeklyAvailability = string.Join(" | ", att.WeeklySlots.Select(s =>
                    {
                        var dayName = s.DayOfWeek >= 0 && s.DayOfWeek < days.Length ? days[s.DayOfWeek] : s.DayOfWeek.ToString();
                        return $"{dayName} ({s.From:HH:mm} - {s.To:HH:mm})";
                    }));
                }
                else
                {
                    WeeklyAvailability = "غير محدد";
                }
            }
            else
            {
                Timezone = string.Empty;
                PreferredSessionDuration = "غير محدد";
                WeeklyAvailability = "غير محدد";
            }

            HasLoadedProfile = true;
        }
        catch (Exception)
        {
            ProfileErrorMessage = "حدث خطأ أثناء تحميل البيانات الشخصية.";
        }
        finally
        {
            IsLoadingProfile = false;
        }
    }

    public void AddReport(SessionReport report)
    {
        SessionHistory.Insert(0, report);
        LastEvaluation = report.Rating;
        TotalMistakesRecorded += report.Mistakes.Total;
        RefreshStats();

        // Update underlying student record if available
        if (_student != null)
        {
            _student = _student with
            {
                HasRecitedToday = true,
                LastRecitedAt = report.CompletedAt,
                LastEvaluation = report.Rating,
                TotalMistakesRecorded = TotalMistakesRecorded
            };
        }
    }

    private void RefreshStats()
    {
        TotalSessions = SessionHistory.Count;
        AverageScore = SessionHistory.Count > 0
            ? Math.Round(SessionHistory.Average(r => r.Score), 1)
            : 0;
    }

    [RelayCommand]
    private void StartMemorizationRecitation()
    {
        if (_student != null)
            RecitationRequested?.Invoke(this, (_student, "حفظ", _student.CurrentMemorizationPage ?? 1));
    }

    [RelayCommand]
    private void StartReviewRecitation()
    {
        if (_student != null)
            RecitationRequested?.Invoke(this, (_student, "\u0645\u0631\u0627\u062c\u0639\u0629", _student.CurrentReviewPage ?? 1));
    }

    [RelayCommand]
    private void StartSardRecitation()
    {
        if (_student != null)
            RecitationRequested?.Invoke(this, (_student, "سرد", _student.CurrentRecitationPage ?? 1));
    }

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);
}
