using System.Text.Json;
using System.Text.Json.Serialization;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Realtime;

internal sealed record SignalingEnvelopeDto(
    [property: JsonPropertyName("message_id")] Guid MessageId,
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("sender_id")] Guid SenderId,
    [property: JsonPropertyName("recipient_id")] Guid RecipientId,
    [property: JsonPropertyName("sender_role")] string SenderRole,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("occurred_at")] DateTimeOffset OccurredAt,
    [property: JsonPropertyName("client_operation_id")] Guid? ClientOperationId,
    [property: JsonPropertyName("payload")] JsonElement Payload);

internal sealed record OfferPayloadDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("sdp")] string Sdp);

internal sealed record AnswerPayloadDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("sdp")] string Sdp);

internal sealed record HostIceCandidatePayloadDto(
    [property: JsonPropertyName("candidate")] string Candidate,
    [property: JsonPropertyName("sdp_mid")] string? SdpMid,
    [property: JsonPropertyName("sdp_m_line_index")] int SdpMLineIndex,
    [property: JsonPropertyName("username_fragment")] string? UsernameFragment);

internal static class SignalingProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false
    };

    public static Result<WebRtcOfferSignal> ParseOffer(SignalingEnvelopeDto envelope)
    {
        if (envelope.Type != "webrtc.offer")
        {
            return Result<WebRtcOfferSignal>.Failure(new AppError(AppErrorKind.Data, "نوع رسالة الإشارة غير متوافق مع Offer."));
        }

        var payload = envelope.Payload.Deserialize<OfferPayloadDto>(JsonOptions);
        if (payload is null || payload.Type != "offer" || string.IsNullOrWhiteSpace(payload.Sdp))
        {
            return Result<WebRtcOfferSignal>.Failure(new AppError(AppErrorKind.Data, "حمولة Offer غير صالحة."));
        }

        return Result<WebRtcOfferSignal>.Success(new WebRtcOfferSignal(envelope.SessionId, envelope.RecipientId, payload.Sdp));
    }

    public static Result<WebRtcAnswerSignal> ParseAnswer(SignalingEnvelopeDto envelope)
    {
        if (envelope.Type != "webrtc.answer")
        {
            return Result<WebRtcAnswerSignal>.Failure(new AppError(AppErrorKind.Data, "نوع رسالة الإشارة غير متوافق مع Answer."));
        }

        var payload = envelope.Payload.Deserialize<AnswerPayloadDto>(JsonOptions);
        if (payload is null || payload.Type != "answer" || string.IsNullOrWhiteSpace(payload.Sdp))
        {
            return Result<WebRtcAnswerSignal>.Failure(new AppError(AppErrorKind.Data, "حمولة Answer غير صالحة."));
        }

        return Result<WebRtcAnswerSignal>.Success(new WebRtcAnswerSignal(envelope.SessionId, envelope.RecipientId, payload.Sdp));
    }

    public static Result<HostIceCandidate> ParseHostIceCandidate(SignalingEnvelopeDto envelope)
    {
        if (envelope.Type != "webrtc.ice_candidate")
        {
            return Result<HostIceCandidate>.Failure(new AppError(AppErrorKind.Data, "نوع رسالة الإشارة غير متوافق مع ICE."));
        }

        var payload = envelope.Payload.Deserialize<HostIceCandidatePayloadDto>(JsonOptions);
        return payload is null
            ? Result<HostIceCandidate>.Failure(new AppError(AppErrorKind.Data, "حمولة ICE غير صالحة."))
            : HostIceCandidatePolicy.Validate(payload.Candidate, payload.SdpMid, payload.SdpMLineIndex, payload.UsernameFragment);
    }
}
