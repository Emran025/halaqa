namespace Halaqa.Desktop.Features.Sessions.Domain.Entities;

public sealed record CreateSessionTaskCommand(
    Guid SessionId,
    SessionTaskType TaskType,
    Guid ClientOperationId,
    int? SequenceNo = null,
    decimal? PlannedAmount = null,
    int? PlannedFromUnitId = null,
    int? PlannedToUnitId = null,
    int? StartPage = null,
    int? StartAyahId = null,
    int? EndPage = null,
    int? EndAyahId = null);

public sealed record UpdateSessionTaskCommand(
    Guid SessionId,
    Guid TaskId,
    int? PlannedFromUnitId = null,
    int? PlannedToUnitId = null,
    int? StartPage = null,
    int? StartAyahId = null,
    int? EndPage = null,
    int? EndAyahId = null,
    int? CurrentPage = null,
    int? CurrentAyahId = null,
    OfficialSessionTaskState? State = null,
    decimal? PlannedAmount = null,
    decimal? ActualAmount = null)
{
    public bool HasChanges =>
        PlannedFromUnitId.HasValue || PlannedToUnitId.HasValue ||
        StartPage.HasValue || StartAyahId.HasValue ||
        EndPage.HasValue || EndAyahId.HasValue ||
        CurrentPage.HasValue || CurrentAyahId.HasValue ||
        State.HasValue || PlannedAmount.HasValue || ActualAmount.HasValue;
}
