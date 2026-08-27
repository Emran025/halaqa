namespace Halaqa.Desktop.Features.Sessions.Domain.Entities;

public enum OfficialSessionTaskState
{
    Draft,
    InProgress,
    Completed,
    Skipped,
    Cancelled
}

public sealed record SessionTaskListItem(
    Guid Id,
    Guid SessionId,
    SessionTaskType TaskType,
    int SequenceNo,
    OfficialSessionTaskState State,
    int? PlannedFromUnitId,
    int? PlannedToUnitId,
    decimal? PlannedAmount,
    decimal? ActualAmount,
    string? Comment,
    int? Score,
    decimal? Gap,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    decimal? TeacherEvaluation,
    decimal? StudentEvaluation,
    int MistakesCount);

public sealed record SessionTaskPage(
    IReadOnlyList<SessionTaskListItem> Tasks,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);
