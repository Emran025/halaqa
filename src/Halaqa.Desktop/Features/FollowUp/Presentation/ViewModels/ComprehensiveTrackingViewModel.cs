using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Features.Progress.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class ComprehensiveTrackingViewModel : ObservableObject
{
    private readonly ListHalaqasUseCase _listHalaqasUseCase;
    private readonly ListHalaqaMembershipsUseCase _listMembershipsUseCase;
    private readonly GetFollowUpPlanUseCase _getPlanUseCase;
    private readonly ListFollowUpItemsUseCase _listItemsUseCase;
    private readonly ListStudentTrackingsUseCase _listTrackingsUseCase;
    private readonly GetStudentProgressUseCase _getProgressUseCase;
    private readonly List<StudentFollowUpSummary> _allStudents = new();
    private bool _hasLoaded;

    [ObservableProperty] private string _selectedFilterTab = "ExpectedToday";
    [ObservableProperty] private string _selectedTaskType = "All";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private int _expectedTodayCount;
    [ObservableProperty] private int _completedTodayCount;
    [ObservableProperty] private int _allStudentsCount;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;

    public ObservableCollection<StudentFollowUpSummary> DisplayedStudents { get; } = new();

    public ComprehensiveTrackingViewModel(
        ListHalaqasUseCase listHalaqasUseCase,
        ListHalaqaMembershipsUseCase listMembershipsUseCase,
        GetFollowUpPlanUseCase getPlanUseCase,
        ListFollowUpItemsUseCase listItemsUseCase,
        ListStudentTrackingsUseCase listTrackingsUseCase,
        GetStudentProgressUseCase getProgressUseCase)
    {
        _listHalaqasUseCase = listHalaqasUseCase;
        _listMembershipsUseCase = listMembershipsUseCase;
        _getPlanUseCase = getPlanUseCase;
        _listItemsUseCase = listItemsUseCase;
        _listTrackingsUseCase = listTrackingsUseCase;
        _getProgressUseCase = getProgressUseCase;
        StartSardRecitationCommand = new RelayCommand<StudentFollowUpSummary?>(StartSardRecitation);
    }

    public event EventHandler? BackRequested;
    public event EventHandler<(StudentFollowUpSummary Student, string TaskType, int TargetPage)>? RecitationRequested;
    public event EventHandler<StudentFollowUpSummary>? ReportsRequested;
    public event EventHandler<StudentFollowUpSummary>? ProfileRequested;

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
                var membershipsResult = await LoadAllMembershipsAsync(halaqa.Id);
                if (!membershipsResult.IsSuccess || membershipsResult.Value is null)
                {
                    IsError = true;
                    Message = membershipsResult.Error?.Message ?? "تعذر تحميل أعضاء الحلقة من الخادم.";
                    continue;
                }

                var summaries = membershipsResult.Value
                    .Where(membership => loadedStudentIds.Add(membership.Student.Id))
                    .Select(membership => BuildSummaryAsync(
                        membership.Student.Id,
                        membership.Student.Name,
                        halaqa.Id,
                        halaqa.Name,
                        today,
                        todayDayOfWeek));
                _allStudents.AddRange(await Task.WhenAll(summaries));
            }

            if (_allStudents.Count == 0 && !IsError)
                Message = "لا توجد عضويات فعالة مسجلة في الحلقات.";

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

    private async Task<Result<IReadOnlyList<HalaqaMembership>>> LoadAllMembershipsAsync(Guid halaqaId)
    {
        var result = await _listMembershipsUseCase.ExecuteAsync(halaqaId, status: "active", page: 1, perPage: 30);
        if (!result.IsSuccess || result.Value is null)
            return Result<IReadOnlyList<HalaqaMembership>>.Failure(result.Error!);

        var memberships = result.Value.Memberships.ToList();
        for (var page = 2; page <= result.Value.LastPage; page++)
        {
            var next = await _listMembershipsUseCase.ExecuteAsync(halaqaId, status: "active", page, perPage: 30);
            if (!next.IsSuccess || next.Value is null)
                return Result<IReadOnlyList<HalaqaMembership>>.Failure(next.Error!);
            memberships.AddRange(next.Value.Memberships);
        }

        return Result<IReadOnlyList<HalaqaMembership>>.Success(memberships);
    }

    private async Task<StudentFollowUpSummary> BuildSummaryAsync(
        Guid studentId,
        string studentName,
        Guid halaqaId,
        string halaqaName,
        DateOnly today,
        int todayDayOfWeek)
    {
        FollowUpPlan? plan = null;
        var planResult = await _getPlanUseCase.ExecuteAsync(studentId);
        if (planResult.IsSuccess)
            plan = planResult.Value;

        var todayItemsResult = await _listItemsUseCase.ExecuteAsync(
            new FollowUpItemQuery(today, null, null, studentId, page: 1, perPage: 50));
        var hasOfficialTodayItems = todayItemsResult.IsSuccess && todayItemsResult.Value?.Items.Count > 0;

        var trackingResult = await _listTrackingsUseCase.ExecuteAsync(studentId, null, null, page: 1, perPage: 1);
        var latestTracking = trackingResult.IsSuccess ? trackingResult.Value?.Items.FirstOrDefault() : null;

        var progressResult = await _getProgressUseCase.ExecuteAsync(studentId, taskType: null);
        var progress = progressResult.IsSuccess ? progressResult.Value : null;

        var todaySlot = plan?.AttendancePreferences.WeeklySlots
            .FirstOrDefault(slot => slot.DayOfWeek == todayDayOfWeek);
        var isScheduledToday = hasOfficialTodayItems || FollowUpSchedulePolicy.IsScheduledOn(plan, today);
        var hasRecitedToday = latestTracking?.Date == today && latestTracking.AttendanceType == AttendanceType.Present;

        return new StudentFollowUpSummary(
            StudentId: studentId,
            StudentName: studentName,
            StudentCode: null,
            HalaqaId: halaqaId,
            HalaqaName: halaqaName,
            Frequency: plan?.Frequency ?? FollowUpFrequency.Unknown,
            AttendanceDay: todaySlot?.DayOfWeek ?? -1,
            AttendanceFrom: todaySlot?.From.ToString("HH:mm") ?? string.Empty,
            AttendanceTo: todaySlot?.To.ToString("HH:mm") ?? string.Empty,
            CurrentMemorizationPage: GetStartPage(progress?.LastCompleted.Memorization),
            CurrentReviewPage: GetStartPage(progress?.LastCompleted.Review),
            CurrentRecitationPage: GetStartPage(progress?.LastCompleted.Recitation),
            IsScheduledToday: isScheduledToday,
            HasRecitedToday: hasRecitedToday,
            LastRecitedAt: latestTracking?.CreatedAt,
            LastEvaluation: latestTracking?.Note,
            TotalMistakesRecorded: progress?.Totals.TotalMistakes ?? 0,
            HasMemorizationPlan: FollowUpSchedulePolicy.HasTaskType(plan, FollowUpTaskType.Memorization),
            HasReviewPlan: FollowUpSchedulePolicy.HasTaskType(plan, FollowUpTaskType.Review),
            HasRecitationPlan: FollowUpSchedulePolicy.HasTaskType(plan, FollowUpTaskType.Recitation));
    }

    private static int? GetStartPage(Halaqa.Desktop.Features.Progress.Domain.Entities.CompletedRecitationRange? range) =>
        range?.StartPage ?? range?.EndPage;

    partial void OnSelectedFilterTabChanged(string value) => ApplyFilters();

    [RelayCommand]
    private void SelectFilterTab(string? tab)
    {
        if (!string.IsNullOrWhiteSpace(tab))
            SelectedFilterTab = tab;
    }
    partial void OnSelectedTaskTypeChanged(string value) => ApplyFilters();
    partial void OnSearchTextChanged(string value) => ApplyFilters();

    private void ApplyFilters()
    {
        ExpectedTodayCount = _allStudents.Count(s => s.IsScheduledToday && !s.HasRecitedToday);
        CompletedTodayCount = _allStudents.Count(s => s.HasRecitedToday);
        AllStudentsCount = _allStudents.Count;

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
            "ExpectedToday" => query.Where(s => s.IsScheduledToday && !s.HasRecitedToday),
            "Completed" => query.Where(s => s.HasRecitedToday),
            _ => query
        };

        DisplayedStudents.Clear();
        foreach (var student in query)
            DisplayedStudents.Add(student);
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

    public IRelayCommand<StudentFollowUpSummary?> StartSardRecitationCommand { get; }

    public void StartSardRecitation(StudentFollowUpSummary? student)
    {
        if (student != null)
            RecitationRequested?.Invoke(this, (student, "سرد", student.CurrentRecitationPage ?? 1));
    }

    [RelayCommand]
    private void OpenReports(StudentFollowUpSummary? student)
    {
        if (student != null)
            ReportsRequested?.Invoke(this, student);
    }

    [RelayCommand]
    private void OpenProfile(StudentFollowUpSummary? student)
    {
        if (student != null)
            ProfileRequested?.Invoke(this, student);
    }

    [RelayCommand]
    public void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    public void MarkStudentCompleted(Guid studentId)
    {
        var index = _allStudents.FindIndex(s => s.StudentId == studentId);
        if (index < 0)
            return;

        var old = _allStudents[index];
        _allStudents[index] = old with
        {
            HasRecitedToday = true,
            LastRecitedAt = DateTimeOffset.Now,
            LastEvaluation = "تم التسميع اليوم"
        };
        ApplyFilters();
    }

    public void MarkStudentCompleted(SessionReport report)
    {
        var index = _allStudents.FindIndex(s => s.StudentId == report.StudentId);
        if (index < 0)
            return;

        var old = _allStudents[index];
        _allStudents[index] = old with
        {
            HasRecitedToday = true,
            LastRecitedAt = report.CompletedAt,
            LastEvaluation = report.Rating,
            TotalMistakesRecorded = old.TotalMistakesRecorded + report.Mistakes.Total
        };
        ApplyFilters();
    }
}
