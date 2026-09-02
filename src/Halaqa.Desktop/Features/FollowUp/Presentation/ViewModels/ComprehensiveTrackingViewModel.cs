﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class ComprehensiveTrackingViewModel : ObservableObject
{
    private readonly ListHalaqasUseCase _listHalaqasUseCase;
    private readonly ListHalaqaMembershipsUseCase _listMembershipsUseCase;
    private readonly List<StudentFollowUpSummary> _allStudents = new();

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
        ListHalaqaMembershipsUseCase listMembershipsUseCase)
    {
        _listHalaqasUseCase = listHalaqasUseCase;
        _listMembershipsUseCase = listMembershipsUseCase;
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
                        var studentIndex = 0;
                        foreach (var m in membershipsResult.Value.Memberships)
                        {
                            studentIndex++;
                            var isScheduled = true;
                            var memPage = ((studentIndex * 3 + 1) % 600) + 1;
                            var revPage = Math.Max(1, memPage - 10);
                            var recPage = Math.Max(1, memPage - 30);

                            var summary = new StudentFollowUpSummary(
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
                                IsScheduledToday: isScheduled,
                                HasRecitedToday: false,
                                LastRecitedAt: DateTimeOffset.Now.AddDays(-1),
                                LastEvaluation: "جيد جداً",
                                TotalMistakesRecorded: 0);

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

    private void PopulateSampleStudents(int todayDayOfWeek)
    {
        var sampleNames = new[]
        {
            ("عبدالله محمد الشامي", 1, 1, 1, false),
            ("عمر أحمد باعباد", 15, 2, 1, false),
            ("يوسف خالد باوزير", 45, 20, 10, true),
            ("أحمد علي السعدي", 80, 50, 25, false),
            ("إبراهيم صالح القحطاني", 120, 90, 60, false),
            ("حمزة طارق العمودي", 200, 160, 100, true)
        };

        foreach (var (name, memPage, revPage, recPage, hasRecited) in sampleNames)
        {
            var studentId = Guid.NewGuid();
            _allStudents.Add(new StudentFollowUpSummary(
                StudentId: studentId,
                StudentName: name,
                StudentCode: $"STU-{studentId.ToString()[..6].ToUpperInvariant()}",
                HalaqaId: Guid.NewGuid(),
                HalaqaName: "حلقة الفجر النموذجية",
                Frequency: FollowUpFrequency.Daily,
                AttendanceDay: todayDayOfWeek,
                AttendanceFrom: "17:30",
                AttendanceTo: "18:30",
                CurrentMemorizationPage: memPage,
                CurrentReviewPage: revPage,
                CurrentRecitationPage: recPage,
                IsScheduledToday: true,
                HasRecitedToday: hasRecited,
                LastRecitedAt: hasRecited ? DateTimeOffset.Now : DateTimeOffset.Now.AddDays(-1),
                LastEvaluation: hasRecited ? "ممتاز (98%)" : "جيد جداً (88%)",
                TotalMistakesRecorded: hasRecited ? 1 : 0));
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
    private void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);

        [RelayCommand]
    private void SelectFilterTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
        {
            SelectedFilterTab = tab;
        }
    }

public void MarkStudentCompleted(Guid studentId)
    {
        var student = _allStudents.FirstOrDefault(s => s.StudentId == studentId);
        if (student != null)
        {
            var updated = student with
            {
                HasRecitedToday = true,
                LastRecitedAt = DateTimeOffset.Now,
                LastEvaluation = "تم التسميع بنجاح"
            };
            var index = _allStudents.IndexOf(student);
            _allStudents[index] = updated;
            ApplyFilters();
        }
    }
}
