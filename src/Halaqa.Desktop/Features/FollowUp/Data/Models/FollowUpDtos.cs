using System.Text.Json;
using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.FollowUp.Data.Models;

internal sealed record WeeklyAvailabilitySlotDto(
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("preferred")] bool Preferred);

internal sealed record AttendancePreferencesDto(
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("weekly_slots")] IReadOnlyList<WeeklyAvailabilitySlotDto> WeeklySlots,
    [property: JsonPropertyName("preferred_session_duration_minutes")] int? PreferredSessionDurationMinutes);

internal sealed record PlanDetailDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("sort_order")] int SortOrder,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

internal sealed record FollowUpPlanDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("created_by_user_id")] Guid CreatedByUserId,
    [property: JsonPropertyName("source_registration_request_id")] Guid? SourceRegistrationRequestId,
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("details")] IReadOnlyList<PlanDetailDto> Details,
    [property: JsonPropertyName("attendance_preferences")] AttendancePreferencesDto? AttendancePreferences = null,
    [property: JsonPropertyName("starts_on")] string? StartsOn = null,
    [property: JsonPropertyName("ends_on")] string? EndsOn = null,
    [property: JsonPropertyName("version")] int Version = 1,
    [property: JsonPropertyName("approved_by_user_id")] Guid? ApprovedByUserId = null,
    [property: JsonPropertyName("approved_at")] DateTimeOffset? ApprovedAt = null,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt = default,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt = default);

internal sealed record FollowUpPlanResponseDto(
    [property: JsonPropertyName("follow_up_plan")] FollowUpPlanDto? FollowUpPlan);

internal sealed record AttendancePreferencesResponseDto(
    [property: JsonPropertyName("attendance_preferences")] AttendancePreferencesDto AttendancePreferences);

internal sealed record PlanDetailInputDto(
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("notes")] string? Notes);

internal sealed record FollowUpPlanInputDto(
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("details")] IReadOnlyList<PlanDetailInputDto> Details,
    [property: JsonPropertyName("starts_on")] string? StartsOn,
    [property: JsonPropertyName("ends_on")] string? EndsOn);

internal sealed record FollowUpItemDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("plan_id")] Guid PlanId,
    [property: JsonPropertyName("plan_detail_id")] Guid PlanDetailId,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("halaqa_id")] Guid? HalaqaId,
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("plan_detail")] PlanDetailDto PlanDetail,
    [property: JsonPropertyName("scheduled_for")] DateTimeOffset ScheduledFor,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("completed_at")] DateTimeOffset? CompletedAt,
    [property: JsonPropertyName("skipped_at")] DateTimeOffset? SkippedAt,
    [property: JsonPropertyName("skip_reason")] string? SkipReason,
    [property: JsonPropertyName("rescheduled_from_id")] Guid? RescheduledFromId,
    [property: JsonPropertyName("notification_sent_at")] DateTimeOffset? NotificationSentAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

internal sealed record PaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record FollowUpItemResponseDto(
    [property: JsonPropertyName("follow_up_item")] FollowUpItemDto FollowUpItem);

internal sealed record FollowUpItemCollectionResponseDto(
    [property: JsonPropertyName("follow_up_items")] IReadOnlyList<FollowUpItemDto> FollowUpItems,
    [property: JsonPropertyName("meta")] PaginationMetaDto Meta);

internal sealed record CompleteFollowUpInputDto(
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);

internal sealed record SkipFollowUpInputDto(
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);

internal sealed record RescheduleFollowUpInputDto(
    [property: JsonPropertyName("scheduled_at")] DateTimeOffset ScheduledAt,
    [property: JsonPropertyName("timezone")] string? Timezone,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);

internal sealed record TrackingDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("halaqa_id")] Guid? HalaqaId,
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("attendance_type")] string AttendanceType,
    [property: JsonPropertyName("note")] string? Note,
    [property: JsonPropertyName("behavior_note")] int? BehaviorNote,
    [property: JsonPropertyName("details")] JsonElement Details,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

internal sealed record TrackingCollectionResponseDto(
    [property: JsonPropertyName("trackings")] IReadOnlyList<TrackingDto> Trackings,
    [property: JsonPropertyName("meta")] PaginationMetaDto Meta);
