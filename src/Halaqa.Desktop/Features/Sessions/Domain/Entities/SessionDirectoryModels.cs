namespace Halaqa.Desktop.Features.Sessions.Domain.Entities;

public enum SessionTaskType
{
    Memorization,
    Review,
    Recitation
}

public enum OfficialSessionState
{
    Requested,
    Accepted,
    Connecting,
    DirectNegotiation,
    Connected,
    WeakConnection,
    Reconnecting,
    Disconnected,
    DirectConnectionUnavailable,
    Ended,
    Cancelled,
    Rejected
}

public sealed record SessionParticipant(
    Guid Id,
    string Role,
    string Name,
    string Email,
    string? Phone,
    string Status);

public sealed record SessionListItem(
    Guid Id,
    Guid HalaqaId,
    SessionParticipant Teacher,
    SessionParticipant Student,
    Guid? FollowUpItemId,
    SessionTaskType TaskType,
    OfficialSessionState State,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset RequestedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? EndedAt,
    string? EndReason,
    bool DirectP2POnly,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateLiveSessionCommand(
    Guid HalaqaId,
    Guid StudentId,
    Guid? FollowUpItemId,
    SessionTaskType TaskType,
    DateTimeOffset? ScheduledAt,
    Guid ClientOperationId);

public sealed record SessionQuery(
    Guid? HalaqaId,
    Guid? StudentId,
    OfficialSessionState? State,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PerPage);

public sealed record SessionPage(
    IReadOnlyList<SessionListItem> Sessions,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);
