using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class StudentsViewModel : ObservableObject
{
    private readonly ListHalaqasUseCase _listHalaqasUseCase;
    private readonly ListHalaqaMembershipsUseCase _listMembershipsUseCase;
    private readonly List<StudentFollowUpSummary> _allStudents = new();

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
        ListHalaqaMembershipsUseCase listMembershipsUseCase)
    {
        _listHalaqasUseCase = listHalaqasUseCase;
        _listMembershipsUseCase = listMembershipsUseCase;
    }

    public async Task InitializeAsync() => await LoadStudentsAsync();

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
            if (halaqasResult.IsSuccess && halaqasResult.Value?.Halaqas != null)
            {
                foreach (var halaqa in halaqasResult.Value.Halaqas)
                {
                    var membershipsResult = await _listMembershipsUseCase.ExecuteAsync(halaqa.Id, status: "active", page: 1);
                    if (membershipsResult.IsSuccess && membershipsResult.Value?.Memberships != null)
                    {
                        var idx = 0;
                        foreach (var m in membershipsResult.Value.Memberships)
                        {
                            idx++;
                            var memPage = ((idx * 3 + 1) % 600) + 1;
                            var revPage = Math.Max(1, memPage - 10);
                            var recPage = Math.Max(1, memPage - 30);
                            _allStudents.Add(new StudentFollowUpSummary(
                                StudentId: m.Student.Id,
                                StudentName: m.Student.Name,
                                StudentCode: $"STU-{m.Student.Id.ToString()[..6].ToUpperInvariant()}",
                                HalaqaId: halaqa.Id,
                                HalaqaName: halaqa.Name,
                                Frequency: FollowUpFrequency.Daily,
                                AttendanceDay: todayDayOfWeek,
                                AttendanceFrom: "18:00",
                                AttendanceTo: "19:00",
                                CurrentMemorizationPage: memPage,
                                CurrentReviewPage: revPage,
                                CurrentRecitationPage: recPage,
                                IsScheduledToday: true,
                                HasRecitedToday: idx % 3 == 0,
                                LastRecitedAt: DateTimeOffset.Now.AddDays(-1),
                                LastEvaluation: "\u062c\u064a\u062f \u062c\u062f\u0627\u064b (4/5)",
                                TotalMistakesRecorded: idx % 5));
                        }
                    }
                }
            }

            if (_allStudents.Count == 0)
                PopulateSampleStudents(todayDayOfWeek);

            UpdateStats();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            IsError = true;
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PopulateSampleStudents(int todayDayOfWeek)
    {
        var data = new[]
        {
            ("\u0639\u0628\u062f\u0627\u0644\u0644\u0647 \u0645\u062d\u0645\u062f \u0627\u0644\u0634\u0627\u0645\u064a",  "\u062d\u0644\u0642\u0629 \u0627\u0644\u0641\u062c\u0631",   1,   1,   1,  false, "\u062c\u064a\u062f (3/5)",       2),
            ("\u0639\u0645\u0631 \u0623\u062d\u0645\u062f \u0628\u0627\u0639\u0628\u0627\u062f",      "\u062d\u0644\u0642\u0629 \u0627\u0644\u0641\u062c\u0631",  15,   2,   1,  false, "\u062c\u064a\u062f \u062c\u062f\u0627\u064b (4/5)",  1),
            ("\u064a\u0648\u0633\u0641 \u062e\u0627\u0644\u062f \u0628\u0627\u0648\u0632\u064a\u0631",     "\u062d\u0644\u0642\u0629 \u0627\u0644\u0635\u0628\u062d",  45,  20,  10,  true,  "\u0645\u0645\u062a\u0627\u0632 (5/5)",  0),
            ("\u0623\u062d\u0645\u062f \u0639\u0644\u064a \u0627\u0644\u0633\u0639\u062f\u064a",      "\u062d\u0644\u0642\u0629 \u0627\u0644\u0635\u0628\u062d",  80,  50,  25,  false, "\u062c\u064a\u062f \u062c\u062f\u0627\u064b (4/5)",  3),
            ("\u0625\u0628\u0631\u0627\u0647\u064a\u0645 \u0635\u0627\u0644\u062d \u0627\u0644\u0642\u062d\u0637\u0627\u0646\u064a","\u062d\u0644\u0642\u0629 \u0627\u0644\u0645\u063a\u0631\u0628", 120,  90,  60,  false, "\u062c\u064a\u062f (3/5)",       4),
            ("\u062d\u0645\u0632\u0629 \u0637\u0627\u0631\u0642 \u0627\u0644\u0639\u0645\u0648\u062f\u064a",    "\u062d\u0644\u0642\u0629 \u0627\u0644\u0645\u063a\u0631\u0628", 200, 160, 100,  true,  "\u0645\u0645\u062a\u0627\u0632 (5/5)",  1),
            ("\u0633\u0639\u062f \u0641\u0647\u062f \u0627\u0644\u063a\u0627\u0645\u062f\u064a",      "\u062d\u0644\u0642\u0629 \u0627\u0644\u0639\u0634\u0627\u0621", 280, 200, 150,  false, "\u0645\u0642\u0628\u0648\u0644 (2/5)",     6),
            ("\u0645\u062d\u0645\u062f \u0639\u0628\u062f\u0627\u0644\u0631\u062d\u0645\u0646 \u0627\u0644\u0632\u0647\u0631\u0627\u0646\u064a","\u062d\u0644\u0642\u0629 \u0627\u0644\u0639\u0634\u0627\u0621",350, 300, 250,  true,  "\u062c\u064a\u062f \u062c\u062f\u0627\u064b (4/5)",  2),
        };

        foreach (var (name, halaqa, mem, rev, rec, done, lastEval, mistakes) in data)
        {
            var id = Guid.NewGuid();
            _allStudents.Add(new StudentFollowUpSummary(
                StudentId: id,
                StudentName: name,
                StudentCode: $"STU-{id.ToString()[..6].ToUpperInvariant()}",
                HalaqaId: Guid.NewGuid(),
                HalaqaName: halaqa,
                Frequency: FollowUpFrequency.Daily,
                AttendanceDay: todayDayOfWeek,
                AttendanceFrom: "17:30",
                AttendanceTo: "18:30",
                CurrentMemorizationPage: mem,
                CurrentReviewPage: rev,
                CurrentRecitationPage: rec,
                IsScheduledToday: true,
                HasRecitedToday: done,
                LastRecitedAt: done ? DateTimeOffset.Now : DateTimeOffset.Now.AddDays(-1),
                LastEvaluation: lastEval,
                TotalMistakesRecorded: mistakes));
        }
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
            "Today"     => query.Where(s => s.IsScheduledToday && !s.HasRecitedToday),
            "Completed" => query.Where(s => s.HasRecitedToday),
            _           => query
        };

        DisplayedStudents.Clear();
        foreach (var s in query)
            DisplayedStudents.Add(s);
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
            RecitationRequested?.Invoke(this, (student, "\u062d\u0641\u0638", student.CurrentMemorizationPage));
    }

    [RelayCommand]
    private void StartReviewRecitation(StudentFollowUpSummary? student)
    {
        if (student != null)
            RecitationRequested?.Invoke(this, (student, "\u0645\u0631\u0627\u062c\u0639\u0629", student.CurrentReviewPage));
    }

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);
}
