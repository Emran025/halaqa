using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.Sessions.Domain.Repositories;

public interface IRealtimeSignalingClient : IAsyncDisposable
{
    event EventHandler<WebRtcOfferSignal>? OfferReceived;
    event EventHandler<WebRtcAnswerSignal>? AnswerReceived;
    event EventHandler<HostIceCandidate>? HostIceCandidateReceived;
    event EventHandler<DirectConnectionUnavailableSignal>? DirectConnectionUnavailable;

    Task ConnectAsync(
        RealtimeSessionConfig config,
        ChannelAuthorization authorization,
        CancellationToken cancellationToken = default);
    Task SendOfferAsync(WebRtcOfferSignal offer, CancellationToken cancellationToken = default);
    Task SendAnswerAsync(WebRtcAnswerSignal answer, CancellationToken cancellationToken = default);
    Task SendHostIceCandidateAsync(HostIceCandidate candidate, CancellationToken cancellationToken = default);
    Task SendRenegotiationAsync(string reason, int attempt, CancellationToken cancellationToken = default);
}

public sealed record WebRtcOfferSignal(Guid SessionId, Guid RecipientId, string Sdp);
public sealed record WebRtcAnswerSignal(Guid SessionId, Guid RecipientId, string Sdp);
public sealed record DirectConnectionUnavailableSignal(Guid SessionId, string Reason);
