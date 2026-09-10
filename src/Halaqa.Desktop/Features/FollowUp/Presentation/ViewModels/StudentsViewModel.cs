using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class StudentsViewModel : ObservableObject
{
    private readonly ListHalaqasUseCase _listHalaqasUseCase;
    private readonly GetHalaqaStudentsSummaryUseCase _getSummaryUseCase;
    private readonly List<StudentFollowUpSummary> _allStudents = new();
    private bool _hasLoaded;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedFilterTab = "All";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private int _totalStudents;
    [ObservableProperty] private int _todayCount;
    [ObservableProperty] private int _completedCount;

    public ObservableCollection<StudentFollowUpSummary> DisplayedStudents { get; } = new();

    public event EventHandler? BackRequested;
    public event EventHandler<StudentFollowUpSummary>? StudentProfileRequested;
    public event EventHandler<(StudentFollowUpSummary Student, string TaskType, int TargetPage)>? RecitationRequested;

    public StudentsViewModel(
        ListHalaqasUseCase listHalaqasUseCase,
        GetHalaqaStudentsSummaryUseCase getSummaryUseCase)
    {
        _listHalaqasUseCase = listHalaqasUseCase;
        _getSummaryUseCase = getSummaryUseCase;
        StartSardRecitationCommand = new RelayCommand<StudentFollowUpSummary?>(StartSardRecitation);
    }

    public IRelayCommand<StudentFollowUpSummary?> StartSardRecitationCommand { get; }

    public async Task InitializeAsync()
    {
        if (_hasLoaded || IsBusy)
            return;

        await LoadStudentsAsync();
    }

    [RelayCommand]
    public async Task LoadStudentsAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        Message = null;
        IsError = false;

        try
        {
            _allStudents.Clear();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayDayOfWeek = (int)DateTime.Today.DayOfWeek;

            // ─── جلب قائمة الحلقات ───────────────────────────────────────────
            var halaqasResult = await LoadAllHalaqasAsync();
            if (!halaqasResult.IsSuccess || halaqasResult.Value is null)
            {
                IsError = true;
                Message = halaqasResult.Error?.Message ?? "تعذر تحميل الحلقات من الخادم.";
                ApplyFilters();
                return;
            }

            var loadedStudentIds = new HashSet<Guid>();

            foreach (var halaqa in halaqasResult.Value)
            {
                // ─── طلب واحد لكل حلقة بدلاً من N×4 طلبات منفصلة ─────────────
                var summaryResult = await _getSummaryUseCase.ExecuteAsync(halaqa.Id);
                if (!summaryResult.IsSuccess || summaryResult.Value is null)
                {
                    IsError = true;
                    Message = summaryResult.Error?.Message ?? "تعذر تحميل بيانات الطلاب من الخادم.";
                    continue;
                }

                foreach (var studentSummary in summaryResult.Value)
                {
                    // تجنب التكرار إذا انتمى الطالب لأكثر من حلقة
                    if (!loadedStudentIds.Add(studentSummary.StudentId))
                        continue;

                    var plan = studentSummary.FollowUpPlan;
                    var latestTracking = studentSummary.RecentTrackings.FirstOrDefault();
                    var hasRecitedToday = latestTracking?.Date == today;

                    var hasTodayItem = studentSummary.RecentFollowUpItems
                        .Any(i => i.ScheduledFor?.Date == today.ToDateTime(TimeOnly.MinValue).Date
                               && i.State is FollowUpItemState.Upcoming
                                          or FollowUpItemState.Due
                                          or FollowUpItemState.InProgress
                                          or FollowUpItemState.Overdue);

                    var isScheduledToday = hasTodayItem || FollowUpSchedulePolicy.IsScheduledOn(plan, today);
                    var progress = studentSummary.Progress;

                    var todaySlot = plan?.AttendancePreferences.WeeklySlots
                        .FirstOrDefault(slot => slot.DayOfWeek == todayDayOfWeek);

                    _allStudents.Add(new StudentFollowUpSummary(
                        StudentId: studentSummary.StudentId,
                        StudentName: studentSummary.StudentName,
                        StudentCode: null,
                        HalaqaId: halaqa.Id,
                        HalaqaName: halaqa.Name,
                        Frequency: plan?.Frequency ?? FollowUpFrequency.Unknown,
                        AttendanceDay: todaySlot?.DayOfWeek ?? -1,
                        AttendanceFrom: todaySlot?.From.ToString("HH:mm") ?? string.Empty,
                        AttendanceTo: todaySlot?.To.ToString("HH:mm") ?? string.Empty,
                        CurrentMemorizationPage: progress.LastMemorizationPage,
                        CurrentReviewPage: progress.LastReviewPage,
                        CurrentRecitationPage: progress.LastRecitationPage,
                        IsScheduledToday: isScheduledToday,
                        HasRecitedToday: hasRecitedToday,
                        LastRecitedAt: null,
                        LastEvaluation: latestTracking?.Notes,
                        TotalMistakesRecorded: progress.TotalMistakes,
                        HasMemorizationPlan: FollowUpSchedulePolicy.HasTaskType(plan, FollowUpTaskType.Memorization),
                        HasReviewPlan: FollowUpSchedulePolicy.HasTaskType(plan, FollowUpTaskType.Review),
                        HasRecitationPlan: FollowUpSchedulePolicy.HasTaskType(plan, FollowUpTaskType.Recitation)));
                }
            }

            if (_allStudents.Count == 0 && !IsError)
                Message = "لا توجد عضويات فعالة مسجلة في الحلقات.";

            UpdateStats();
            ApplyFilters();
            _hasLoaded = true;
        }
        catch (Exception ex)
        {
            IsError = true;
            Message = $"حدث خطأ أثناء تحميل بيانات الطلاب: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Result<IReadOnlyList<HalaqaItem>>> LoadAllHalaqasAsync()
    {
        var result = await _listHalaqasUseCase.ExecuteAsync(1);
        if (!result.IsSuccess || result.Value is null)
            return Result<IReadOnlyList<HalaqaItem>>.Failure(result.Error!);

        var halaqas = result.Value.Halaqas.ToList();
        for (var page = 2; page <= result.Value.LastPage; page++)
        {
            var next = await _listHalaqasUseCase.ExecuteAsync(page);
            if (!next.IsSuccess || next.Value is null)
                return Result<IReadOnlyList<HalaqaItem>>.Failure(next.Error!);
            halaqas.AddRange(next.Value.Halaqas);
        }

        return Result<IReadOnlyList<HalaqaItem>>.Success(halaqas);
    }

    private void UpdateStats()
    {
        TotalStudents = _allStudents.Count;
        TodayCount = _allStudents.Count(s => s.IsScheduledToday && !s.HasRecitedToday);
        CompletedCount = _allStudents.Count(s => s.HasRecitedToday);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterTabChanged(string value) => ApplyFilters();

    private void ApplyFilters()
    {
        var query = _allStudents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(s =>
                s.StudentName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (s.StudentCode != null && s.StudentCode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (s.HalaqaName != null && s.HalaqaName.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        query = SelectedFilterTab switch
        {
            "Today" => query.Where(s => s.IsScheduledToday && !s.HasRecitedToday),
            "Completed" => query.Where(s => s.HasRecitedToday),
            _ => query
        };

        DisplayedStudents.Clear();
        foreach (var student in query)
            DisplayedStudents.Add(student);
    }

    [RelayCommand]
    private void SelectFilterTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
            SelectedFilterTab = tab;
    }

    [RelayCommand]
    private void OpenStudentProfile(StudentFollowUpSummary? student)
    {
        if (student != null)
            StudentProfileRequested?.Invoke(this, student);
    }

    [RelayCommand]
    private void StartMemorizationRecitation(StudentFollowUpSummary? student)
    {
        if (student != null)
            RecitationRequested?.Invoke(this, (student, "حفظ", student.CurrentMemorizationPage ?? 1));
    }

    [RelayCommand]
    private void StartReviewRecitation(StudentFollowUpSummary? student)
    {
        if (student != null)
            RecitationRequested?.Invoke(this, (student, "مراجعة", student.CurrentReviewPage ?? 1));
    }

    public void StartSardRecitation(StudentFollowUpSummary? student)
    {
        if (student != null)
            RecitationRequested?.Invoke(this, (student, "سرد", student.CurrentRecitationPage ?? 1));
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);
}
