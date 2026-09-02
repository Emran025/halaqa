using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.Repositories;

public interface ILiveSessionRepository
{
    Task<Result<RealtimeSessionConfig>> GetRealtimeConfigAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<ChannelAuthorization>> AuthorizeChannelAsync(
        Guid sessionId,
        string channelName,
        string? clientConnectionId,
        CancellationToken cancellationToken = default);
    Task<Result> SaveOfficialMushafStateAsync(
        Guid sessionId,
        int editionId,
        int pageNumber,
        int? ayahId,
        Guid clientOperationId,
        CancellationToken cancellationToken = default);
}

public sealed record ChannelAuthorization(
    bool IsAuthorized,
    Guid SessionId,
    string ChannelName,
    Guid RecipientId,
    DateTimeOffset? ExpiresAt);

public interface IPeerMediaConnection : IDisposable, IAsyncDisposable
{
    event EventHandler<PeerConnectionStateChangedEventArgs>? StateChanged;
    event EventHandler<PeerMediaStateChangedEventArgs>? RemoteMediaStateChanged;

    Task InitializeAsync(RealtimeSessionConfig config, CancellationToken cancellationToken = default);
    Task CreateOfferAsync(CancellationToken cancellationToken = default);
    Task HandleOfferAsync(string sdp, CancellationToken cancellationToken = default);
    Task HandleAnswerAsync(string sdp, CancellationToken cancellationToken = default);
    Task HandleHostIceCandidateAsync(HostIceCandidate candidate, CancellationToken cancellationToken = default);
    Task SetMicrophoneMutedAsync(bool isMuted, CancellationToken cancellationToken = default);
    Task SetCameraEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default);
}

public sealed record HostIceCandidate(
    string Candidate,
    string? SdpMid,
    int SdpMLineIndex,
    string? UsernameFragment);

public sealed record PeerConnectionStateChangedEventArgs(LiveSessionState State, string? Reason = null);
public sealed record PeerMediaStateChangedEventArgs(bool IsMicrophoneMuted, bool IsCameraEnabled);

public interface IMushafRealtimeChannel : IDisposable, IAsyncDisposable
{
    event EventHandler<MushafPresenceState>? PresenceReceived;
    event EventHandler<PeerRepeatRequest>? RepeatRequested;

    Task SendPresenceAsync(MushafPresenceState state, CancellationToken cancellationToken = default);
    Task SendRepeatRequestAsync(PeerRepeatRequest request, CancellationToken cancellationToken = default);
}

public sealed record PeerRepeatRequest(
    Guid SessionId,
    Guid TaskId,
    int? AyahId,
    string? Reason);

public interface ILocalVideoRecorder : IDisposable, IAsyncDisposable
{
    event EventHandler<LocalRecordingState>? StateChanged;

    Task StartAsync(string outputDirectory, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
