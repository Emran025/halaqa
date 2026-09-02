using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Realtime;

public sealed class DefaultPeerMediaConnection : IPeerMediaConnection
{
    public event EventHandler<PeerConnectionStateChangedEventArgs>? StateChanged;
    public event EventHandler<PeerMediaStateChangedEventArgs>? RemoteMediaStateChanged;

    public Task InitializeAsync(RealtimeSessionConfig config, CancellationToken cancellationToken = default)
    {
        StateChanged?.Invoke(this, new PeerConnectionStateChangedEventArgs(LiveSessionState.DirectConnectionUnavailable, "ط§ظ„ط§طھطµط§ظ„ ط§ظ„ظ…ط¨ط§ط´ط± ط¬ط§ظ‡ط² ظ„ظ„طھظپط§ظˆط¶."));
        return Task.CompletedTask;
    }

    public Task CreateOfferAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task HandleOfferAsync(string sdp, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task HandleAnswerAsync(string sdp, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task HandleHostIceCandidateAsync(HostIceCandidate candidate, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SetMicrophoneMutedAsync(bool isMuted, CancellationToken cancellationToken = default)
    {
        RemoteMediaStateChanged?.Invoke(this, new PeerMediaStateChangedEventArgs(isMuted, false));
        return Task.CompletedTask;
    }

    public Task SetCameraEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default)
    {
        RemoteMediaStateChanged?.Invoke(this, new PeerMediaStateChangedEventArgs(false, isEnabled));
        return Task.CompletedTask;
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}