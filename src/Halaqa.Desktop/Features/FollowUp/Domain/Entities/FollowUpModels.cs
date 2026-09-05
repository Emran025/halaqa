namespace Halaqa.Desktop.Features.FollowUp.Domain.Entities;

public enum FollowUpFrequency
{
    Unknown,
    Daily,
    OnceAWeek,
    TwiceAWeek,
    ThriceAWeek
}

public enum FollowUpTaskType
{
    Memorization,
    Review,
    Recitation
}

public enum FollowUpUnit
{
    Juz,
    Hizb,
    HalfHizb,
    QuarterHizb,
    Page
}

public enum FollowUpItemState
{
    Upcoming,
    Due,
    InProgress,
    Completed,
    Skipped,
    Overdue
}

public enum AttendanceType
{
    Present,
    Absent,
    Excused,
    Late
}

public sealed record WeeklyAvailabilitySlot(
    int DayOfWeek,
    TimeOnly From,
    TimeOnly To,
    bool Preferred);

public sealed record AttendancePreferences(
    string Timezone,
    IReadOnlyList<WeeklyAvailabilitySlot> WeeklySlots,
    int? PreferredSessionDurationMinutes);

public sealed record FollowUpPlanDetail(
    Guid Id,
    FollowUpTaskType TaskType,
    FollowUpUnit Unit,
    decimal Amount,
    string? Notes,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record FollowUpPlan(
    Guid Id,
    Guid StudentId,
    Guid CreatedByUserId,
    Guid? SourceRegistrationRequestId,
    FollowUpFrequency Frequency,
    string Status,
    string Timezone,
    IReadOnlyList<FollowUpPlanDetail> Details,
    AttendancePreferences AttendancePreferences,
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    int Version,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record FollowUpItem(
    Guid Id,
    Guid PlanId,
    Guid PlanDetailId,
    Guid StudentId,
    Guid? HalaqaId,
    FollowUpTaskType TaskType,
    FollowUpPlanDetail PlanDetail,
    DateTimeOffset ScheduledFor,
    string Timezone,
    FollowUpItemState State,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? SkippedAt,
    string? SkipReason,
    Guid? RescheduledFromId,
    DateTimeOffset? NotificationSentAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record FollowUpItemPage(
    IReadOnlyList<FollowUpItem> Items,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record TrackingItem(
    Guid Id,
    Guid StudentId,
    Guid? HalaqaId,
    DateOnly Date,
    AttendanceType AttendanceType,
    string? Note,
    int? BehaviorNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrackingPage(
    IReadOnlyList<TrackingItem> Items,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record PlanDetailDraft(
    FollowUpTaskType TaskType,
    FollowUpUnit Unit,
    decimal Amount,
    string? Notes);

public sealed record UpdateFollowUpPlanCommand(
    Guid StudentId,
    FollowUpFrequency Frequency,
    IReadOnlyList<PlanDetailDraft> Details,
    DateOnly? StartsOn,
    DateOnly? EndsOn);

public sealed record UpdateAvailabilityCommand(
    Guid StudentId,
    AttendancePreferences Preferences);

public sealed record FollowUpItemQuery(
    DateOnly? Date,
    FollowUpItemState? State,
    FollowUpTaskType? TaskType,
    Guid? StudentId,
    int Page,
    int PerPage);

public sealed record RescheduleFollowUpItemCommand(
    Guid ItemId,
    DateTimeOffset ScheduledAt,
    string? Timezone,
    string? Reason,
    Guid ClientOperationId);

public sealed record StudentFollowUpSummary(
    Guid StudentId,
    string StudentName,
    string? StudentCode,
    Guid? HalaqaId,
    string? HalaqaName,
    FollowUpFrequency Frequency,
    int AttendanceDay,
    string AttendanceFrom,
    string AttendanceTo,
    int? CurrentMemorizationPage,
    int? CurrentReviewPage,
    int? CurrentRecitationPage,
    bool IsScheduledToday,
    bool HasRecitedToday,
    DateTimeOffset? LastRecitedAt,
    string? LastEvaluation,
    int TotalMistakesRecorded = 0,
    bool HasMemorizationPlan = false,
    bool HasReviewPlan = false,
    bool HasRecitationPlan = false);
