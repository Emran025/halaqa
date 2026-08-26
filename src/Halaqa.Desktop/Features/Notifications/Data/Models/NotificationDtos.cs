using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Notifications.Data.Models;

internal sealed record NotificationPayloadDto(
    [property: JsonPropertyName("event_type")] string? EventType,
    [property: JsonPropertyName("entity_type")] string EntityType,
    [property: JsonPropertyName("entity_id")] Guid EntityId,
    [property: JsonPropertyName("session_id")] Guid? SessionId,
    [property: JsonPropertyName("follow_up_item_id")] Guid? FollowUpItemId,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("action_path")] string? ActionPath);

internal sealed record NotificationDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("payload")] NotificationPayloadDto Payload,
    [property: JsonPropertyName("read_at")] DateTimeOffset? ReadAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

internal sealed record NotificationPaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record NotificationCollectionResponseDto(
    [property: JsonPropertyName("notifications")] IReadOnlyList<NotificationDto> Notifications,
    [property: JsonPropertyName("meta")] NotificationPaginationMetaDto Meta);
