namespace Halaqa.Desktop.Features.Sessions.Domain.Entities;

/// <summary>
/// Immutable report produced after a live recitation session ends.
/// Carries all data needed to update the student's tracking card and profile history.
/// </summary>
public sealed record SessionMistakeSummary(
    int MemorizationCount,
    int TajweedCount,
    int TashkeelCount,
    int AlertCount)
{
    public int Total => MemorizationCount + TajweedCount + TashkeelCount + AlertCount;
}

public sealed record SessionReport(
    Guid StudentId,
    string StudentName,
    string TaskType,
    int TargetPage,
    int? StopAyahNumber,
    SessionMistakeSummary Mistakes,
    int Score,
    string Rating,
    string Notes,
    DateTimeOffset CompletedAt);
