using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class StudentRecitationProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _studentName = string.Empty;
    [ObservableProperty] private string _studentCode = string.Empty;
    [ObservableProperty] private string _halaqaName = string.Empty;
    [ObservableProperty] private int _currentMemorizationPage;
    [ObservableProperty] private int _currentReviewPage;
    [ObservableProperty] private int _currentRecitationPage;
    [ObservableProperty] private string _lastEvaluation = string.Empty;
    [ObservableProperty] private int _totalMistakesRecorded;
    [ObservableProperty] private double _averageScore;
    [ObservableProperty] private int _totalSessions;
    [ObservableProperty] private string _attendanceFrom = string.Empty;
    [ObservableProperty] private string _attendanceTo = string.Empty;

    private StudentFollowUpSummary? _student;

    public ObservableCollection<SessionReport> SessionHistory { get; } = new();

    public event EventHandler? BackRequested;
    public event EventHandler<(StudentFollowUpSummary Student, string TaskType, int TargetPage)>? RecitationRequested;

    public void Initialize(StudentFollowUpSummary student, IReadOnlyList<SessionReport> reports)
    {
        _student = student;
        StudentName = student.StudentName;
        StudentCode = student.StudentCode ?? $"STU-{student.StudentId.ToString()[..6].ToUpperInvariant()}";
        HalaqaName = student.HalaqaName ?? string.Empty;
        CurrentMemorizationPage = student.CurrentMemorizationPage;
        CurrentReviewPage = student.CurrentReviewPage;
        CurrentRecitationPage = student.CurrentRecitationPage;
        LastEvaluation = student.LastEvaluation ?? string.Empty;
        TotalMistakesRecorded = student.TotalMistakesRecorded;
        AttendanceFrom = student.AttendanceFrom;
        AttendanceTo = student.AttendanceTo;

        SessionHistory.Clear();
        foreach (var r in reports.OrderByDescending(r => r.CompletedAt))
            SessionHistory.Add(r);

        RefreshStats();
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
            RecitationRequested?.Invoke(this, (_student, "\u062d\u0641\u0638", _student.CurrentMemorizationPage));
    }

    [RelayCommand]
    private void StartReviewRecitation()
    {
        if (_student != null)
            RecitationRequested?.Invoke(this, (_student, "\u0645\u0631\u0627\u062c\u0639\u0629", _student.CurrentReviewPage));
    }

    [RelayCommand]
    private void StartSardRecitation()
    {
        if (_student != null)
            RecitationRequested?.Invoke(this, (_student, "\u0633\u0631\u062f", _student.CurrentRecitationPage));
    }

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);
}
