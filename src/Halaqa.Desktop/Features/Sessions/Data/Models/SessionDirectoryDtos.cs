using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Sessions.Data.Models;

internal sealed record SessionParticipantDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("phone")] string? Phone = null,
    [property: JsonPropertyName("status")] string? Status = null);

internal sealed record SessionListItemDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("halaqa_id")] Guid HalaqaId,
    [property: JsonPropertyName("teacher")] SessionParticipantDto Teacher,
    [property: JsonPropertyName("student")] SessionParticipantDto Student,
    [property: JsonPropertyName("follow_up_item_id")] Guid? FollowUpItemId,
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("scheduled_at")] DateTimeOffset? ScheduledAt,
    [property: JsonPropertyName("requested_at")] DateTimeOffset RequestedAt,
    [property: JsonPropertyName("accepted_at")] DateTimeOffset? AcceptedAt,
    [property: JsonPropertyName("connected_at")] DateTimeOffset? ConnectedAt,
    [property: JsonPropertyName("ended_at")] DateTimeOffset? EndedAt,
    [property: JsonPropertyName("end_reason")] string? EndReason,
    [property: JsonPropertyName("direct_p2p_only")] bool DirectP2POnly,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

internal sealed record SessionPaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record SessionCollectionResponseDto(
    [property: JsonPropertyName("sessions")] IReadOnlyList<SessionListItemDto> Sessions,
    [property: JsonPropertyName("meta")] SessionPaginationMetaDto Meta);
