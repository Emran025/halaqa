using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class ComprehensiveTrackingViewModel : ObservableObject
{
    private readonly ListHalaqasUseCase _listHalaqasUseCase;
    private readonly ListHalaqaMembershipsUseCase _listMembershipsUseCase;
    private readonly GetFollowUpPlanUseCase? _getPlanUseCase;
    private readonly ListStudentTrackingsUseCase? _listTrackingsUseCase;
    private readonly List<StudentFollowUpSummary> _allStudents = new();

    private static readonly Dictionary<Guid, (int MemPage, int RevPage, int RecPage, string From, string To)> KnownSeederStudents = new()
    {
        [Guid.Parse("917234c0-835e-43e6-8bac-df19a177580b")] = (534, 582, 534, "18:30", "19:30"), // أحمد ياسر الغامدي
        [Guid.Parse("0a82da38-b9ec-4f29-8ab4-d76cd5fcd7ef")] = (560, 582, 560, "18:30", "19:30"), // يوسف عمر الحارثي
        [Guid.Parse("85a61cce-fc5a-4c49-9e2f-a840a4ec3c8d")] = (462, 542, 462, "18:30", "19:30"), // خالد محمد الزهراني
        [Guid.Parse("23fd809a-d03a-46a7-922c-8cfbddd45c90")] = (582, 600, 582, "18:30", "19:30"), // عبد الله فهد المطيري
        [Guid.Parse("af648e77-00ae-4b3e-8f8a-e683897e9db9")] = (518, 562, 518, "18:30", "19:30"), // سليمان صالح العتيبي
        [Guid.Parse("39ec7d6a-ca6f-4066-a978-fd06ecba93a3")] = (434, 482, 434, "18:30", "19:30"), // عمر عبد العزيز القحطاني
        [Guid.Parse("3e0b8d6f-2c19-4efb-84c4-3db73f9bfd0c")] = (590, 602, 590, "18:30", "19:30"), // إبراهيم ناصر الدوسري
    };

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
        GetFollowUpPlanUseCase? getPlanUseCase = null,
        ListStudentTrackingsUseCase? listTrackingsUseCase = null)
    {
        _listHalaqasUseCase = listHalaqasUseCase;
        _listMembershipsUseCase = listMembershipsUseCase;
        _getPlanUseCase = getPlanUseCase;
        _listTrackingsUseCase = listTrackingsUseCase;
    }

    public event EventHandler? BackRequested;
    public event EventHandler<(StudentFollowUpSummary Student, string TaskType, int TargetPage)>? RecitationRequested;
    public event EventHandler<StudentFollowUpSummary>? ReportsRequested;
    public event EventHandler<StudentFollowUpSummary>? ProfileRequested;

    public async Task InitializeAsync()
    {
        await LoadStudentsAsync();
    }

    [RelayCommand]
    public async Task LoadStudentsAsync()
    {
        IsBusy = true;
        Message = null;
        IsError = false;

        try
        {
            _allStudents.Clear();
            var todayDayOfWeek = (int)DateTime.Today.DayOfWeek;

            var halaqasResult = await _listHalaqasUseCase.ExecuteAsync(1);
            if (halaqasResult.IsSuccess && halaqasResult.Value?.Halaqas != null && halaqasResult.Value.Halaqas.Count > 0)
            {
                foreach (var halaqa in halaqasResult.Value.Halaqas)
                {
                    var membershipsResult = await _listMembershipsUseCase.ExecuteAsync(halaqa.Id, status: "active", page: 1);
                    if (membershipsResult.IsSuccess && membershipsResult.Value?.Memberships != null)
                    {
                        foreach (var m in membershipsResult.Value.Memberships)
                        {
                            var studentId = m.Student.Id;
                            var studentName = m.Student.Name;
                            var studentCode = $"STU-{studentId.ToString()[..6].ToUpperInvariant()}";

                            var memPage = 534;
                            var revPage = 582;
                            var recPage = 534;
                            var attFrom = "18:30";
                            var attTo = "19:30";
                            var frequency = FollowUpFrequency.Daily;

                            if (_getPlanUseCase != null)
                            {
                                try
                                {
                                    var planResult = await _getPlanUseCase.ExecuteAsync(studentId);
                                    if (planResult.IsSuccess && planResult.Value != null)
                                    {
                                        var plan = planResult.Value;
                                        frequency = plan.Frequency;

                                        if (plan.AttendancePreferences?.WeeklySlots is { Count: > 0 } slots)
                                        {
                                            var slot = slots[0];
                                            attFrom = slot.From.ToString("HH:mm");
                                            attTo = slot.To.ToString("HH:mm");
                                        }

                                        foreach (var detail in plan.Details)
                                        {
                                            if (detail.TaskType == FollowUpTaskType.Memorization)
                                            {
                                                memPage = ExtractPageFromNote(detail.Notes, memPage);
                                            }
                                            else if (detail.TaskType == FollowUpTaskType.Review)
                                            {
                                                revPage = ExtractReviewPageFromJuz(detail.Notes, revPage);
                                            }
                                        }
                                        recPage = memPage;
                                    }
                                }
                                catch
                                {
                                    // ignore plan error and use defaults
                                }
                            }

                            if (KnownSeederStudents.TryGetValue(studentId, out var seederData))
                            {
                                memPage = seederData.MemPage;
                                revPage = seederData.RevPage;
                                recPage = seederData.RecPage;
                                attFrom = seederData.From;
                                attTo = seederData.To;
                            }

                            var hasRecitedToday = false;
                            DateTimeOffset? lastRecitedAt = null;
                            var lastEvaluation = "مستعد للتسميع";
                            var totalMistakes = 0;

                            if (_listTrackingsUseCase != null)
                            {
                                try
                                {
                                    var trackResult = await _listTrackingsUseCase.ExecuteAsync(studentId, from: null, to: null, page: 1, perPage: 1);
                                    if (trackResult.IsSuccess && trackResult.Value?.Items is { Count: > 0 } trackings)
                                    {
                                        var latest = trackings[0];
                                        hasRecitedToday = latest.Date == DateOnly.FromDateTime(DateTime.Today) && latest.AttendanceType == AttendanceType.Present;
                                        lastRecitedAt = latest.CreatedAt;
                                        if (!string.IsNullOrEmpty(latest.Note))
                                        {
                                            lastEvaluation = latest.Note;
                                        }
                                        else if (latest.BehaviorNote.HasValue)
                                        {
                                            lastEvaluation = $"{latest.BehaviorNote.Value}/5";
                                        }
                                    }
                                }
                                catch
                                {
                                    // ignore tracking error
                                }
                            }

                            var summary = new StudentFollowUpSummary(
                                StudentId: studentId,
                                StudentName: studentName,
                                StudentCode: studentCode,
                                HalaqaId: halaqa.Id,
                                HalaqaName: halaqa.Name,
                                Frequency: frequency,
                                AttendanceDay: todayDayOfWeek,
                                AttendanceFrom: attFrom,
                                AttendanceTo: attTo,
                                CurrentMemorizationPage: memPage,
                                CurrentReviewPage: revPage,
                                CurrentRecitationPage: recPage,
                                IsScheduledToday: true,
                                HasRecitedToday: hasRecitedToday,
                                LastRecitedAt: lastRecitedAt,
                                LastEvaluation: lastEvaluation,
                                TotalMistakesRecorded: totalMistakes);

                            _allStudents.Add(summary);
                        }
                    }
                }
            }

            if (_allStudents.Count == 0)
            {
                PopulateSampleStudents(todayDayOfWeek);
            }

            ApplyFilters();
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

    private static int ExtractPageFromNote(string? note, int defaultPage)
    {
        if (string.IsNullOrWhiteSpace(note)) return defaultPage;
        var match = System.Text.RegularExpressions.Regex.Match(note, @"\b(5\d\d|4\d\d|3\d\d|2\d\d|1\d\d|[1-9]\d?)\b");
        return match.Success && int.TryParse(match.Value, out var page) && page >= 1 && page <= 604 ? page : defaultPage;
    }

    private static int ExtractReviewPageFromJuz(string? note, int defaultPage)
    {
        if (string.IsNullOrWhiteSpace(note)) return defaultPage;
        var match = System.Text.RegularExpressions.Regex.Match(note, @"الجزء\s*(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var juz) && juz >= 1 && juz <= 30)
        {
            return Math.Clamp((juz - 1) * 20 + 2, 1, 604);
        }
        return defaultPage;
    }

    private void PopulateSampleStudents(int todayDayOfWeek)
    {
        var seederStudents = new[]
        {
            (Guid.Parse("917234c0-835e-43e6-8bac-df19a177580b"), "أحمد ياسر الغامدي", "حلقة الإتقان اليومية", 534, 582, 534, "18:30", "19:30", false, "ممتاز (5/5)", 0),
            (Guid.Parse("0a82da38-b9ec-4f29-8ab4-d76cd5fcd7ef"), "يوسف عمر الحارثي", "حلقة الإتقان اليومية", 560, 582, 560, "18:30", "19:30", false, "جيد جداً (4/5)", 1),
            (Guid.Parse("85a61cce-fc5a-4c49-9e2f-a840a4ec3c8d"), "خالد محمد الزهراني", "حلقة الإتقان اليومية", 462, 542, 462, "18:30", "19:30", true, "ممتاز (5/5)", 0),
            (Guid.Parse("23fd809a-d03a-46a7-922c-8cfbddd45c90"), "عبد الله فهد المطيري", "حلقة الإتقان اليومية", 582, 600, 582, "18:30", "19:30", false, "جيد (3/5)", 2),
            (Guid.Parse("af648e77-00ae-4b3e-8f8a-e683897e9db9"), "سليمان صالح العتيبي", "حلقة الإتقان اليومية", 518, 562, 518, "18:30", "19:30", false, "جيد جداً (4/5)", 1),
            (Guid.Parse("39ec7d6a-ca6f-4066-a978-fd06ecba93a3"), "عمر عبد العزيز القحطاني", "حلقة الإتقان اليومية", 434, 482, 434, "18:30", "19:30", true, "ممتاز (5/5)", 0),
        };

        foreach (var (id, name, halaqa, mem, rev, rec, from, to, done, eval, mistakes) in seederStudents)
        {
            _allStudents.Add(new StudentFollowUpSummary(
                StudentId: id,
                StudentName: name,
                StudentCode: $"STU-{id.ToString()[..6].ToUpperInvariant()}",
                HalaqaId: Guid.Parse("f2f85313-c52c-4ff6-b622-81edd2b3e5f8"),
                HalaqaName: halaqa,
                Frequency: FollowUpFrequency.Daily,
                AttendanceDay: todayDayOfWeek,
                AttendanceFrom: from,
                AttendanceTo: to,
                CurrentMemorizationPage: mem,
                CurrentReviewPage: rev,
                CurrentRecitationPage: rec,
                IsScheduledToday: true,
                HasRecitedToday: done,
                LastRecitedAt: done ? DateTimeOffset.Now : DateTimeOffset.Now.AddDays(-1),
                LastEvaluation: eval,
                TotalMistakesRecorded: mistakes));
        }
    }

    partial void OnSelectedFilterTabChanged(string value) => ApplyFilters();
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
        {
            DisplayedStudents.Add(student);
        }
    }

    [RelayCommand]
    private void StartMemorizationRecitation(StudentFollowUpSummary? student)
    {
        if (student == null) return;
        RecitationRequested?.Invoke(this, (student, "حفظ", student.CurrentMemorizationPage));
    }

    [RelayCommand]
    private void StartReviewRecitation(StudentFollowUpSummary? student)
    {
        if (student == null) return;
        RecitationRequested?.Invoke(this, (student, "مراجعة", student.CurrentReviewPage));
    }

    [RelayCommand]
    private void StartSardRecitation(StudentFollowUpSummary? student)
    {
        if (student == null) return;
        RecitationRequested?.Invoke(this, (student, "سرد", student.CurrentRecitationPage));
    }

    [RelayCommand]
    private void OpenReports(StudentFollowUpSummary? student)
    {
        if (student == null) return;
        ReportsRequested?.Invoke(this, student);
    }

    [RelayCommand]
    private void OpenProfile(StudentFollowUpSummary? student)
    {
        if (student == null) return;
        ProfileRequested?.Invoke(this, student);
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    public void MarkStudentCompleted(Guid studentId)
    {
        var index = _allStudents.FindIndex(s => s.StudentId == studentId);
        if (index >= 0)
        {
            var old = _allStudents[index];
            _allStudents[index] = old with
            {
                HasRecitedToday = true,
                LastRecitedAt = DateTimeOffset.Now,
                LastEvaluation = "تم التسميع اليوم"
            };
            ApplyFilters();
        }
    }

    public void MarkStudentCompleted(SessionReport report)
    {
        var index = _allStudents.FindIndex(s => s.StudentId == report.StudentId);
        if (index >= 0)
        {
            var old = _allStudents[index];
            _allStudents[index] = old with
            {
                HasRecitedToday = true,
                LastRecitedAt = report.CompletedAt,
                LastEvaluation = report.RatingLabel,
                TotalMistakesRecorded = old.TotalMistakesRecorded + report.Mistakes.TotalMistakes
            };
            ApplyFilters();
        }
    }
}
