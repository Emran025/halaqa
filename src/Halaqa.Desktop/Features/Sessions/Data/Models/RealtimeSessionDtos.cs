using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Sessions.Data.Models;

internal sealed record RealtimeSessionResponseDto(
    [property: JsonPropertyName("realtime_session")] RealtimeSessionDto RealtimeSession);

internal sealed record RealtimeSessionDto(
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("channel_name")] string ChannelName,
    [property: JsonPropertyName("websocket_url")] Uri WebSocketUrl,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("direct_p2p_only")] bool DirectP2POnly,
    [property: JsonPropertyName("signaling_transport")] string SignalingTransport,
    [property: JsonPropertyName("ice_candidate_policy")] string IceCandidatePolicy,
    [property: JsonPropertyName("media_transport")] string? MediaTransport);

internal sealed record AuthorizeRealtimeChannelRequestDto(
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("channel_name")] string ChannelName,
    [property: JsonPropertyName("client_connection_id")] string? ClientConnectionId);

internal sealed record RealtimeChannelAuthorizationResponseDto(
    [property: JsonPropertyName("authorization")] ChannelAuthorizationDto Authorization);

internal sealed record ChannelAuthorizationDto(
    [property: JsonPropertyName("authorized")] bool Authorized,
    [property: JsonPropertyName("channel_name")] string ChannelName,
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("recipient_id")] Guid RecipientId,
    [property: JsonPropertyName("expires_at")] DateTimeOffset? ExpiresAt);

internal sealed record SaveMushafStateRequestDto(
    [property: JsonPropertyName("edition_id")] int EditionId,
    [property: JsonPropertyName("page_number")] int PageNumber,
    [property: JsonPropertyName("ayah_id")] int? AyahId,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);
