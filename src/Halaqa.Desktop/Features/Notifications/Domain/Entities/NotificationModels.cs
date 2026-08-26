namespace Halaqa.Desktop.Features.Notifications.Domain.Entities;

public enum NotificationType
{
    RegistrationRequest,
    SessionScheduled,
    SessionStarted,
    SessionEnded,
    ReportReady,
    FollowUpDue,
    Reminder,
    System
}

public enum NotificationEntityType
{
    RegistrationRequest,
    Halaqa,
    Membership,
    FollowUpItem,
    LiveSession,
    SessionReport,
    Task,
    Mistake
}

public enum NotificationAction
{
    Open,
    Accept,
    Reject,
    Join,
    Review,
    Acknowledge,
    Reschedule
}

public sealed record NotificationPayload(
    string? EventType,
    NotificationEntityType EntityType,
    Guid EntityId,
    Guid? SessionId,
    Guid? FollowUpItemId,
    NotificationAction Action,
    string? ActionPath);

public sealed record HalaqaNotification(
    Guid Id,
    NotificationType Type,
    string Title,
    string Body,
    NotificationPayload Payload,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt)
{
    public bool IsRead => ReadAt is not null;
}

public sealed record NotificationPage(
    IReadOnlyList<HalaqaNotification> Notifications,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record NotificationQuery(bool UnreadOnly, int Page, int PerPage);
