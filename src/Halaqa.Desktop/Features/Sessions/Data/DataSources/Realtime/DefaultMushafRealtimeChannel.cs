using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Realtime;

public sealed class DefaultMushafRealtimeChannel : IMushafRealtimeChannel
{
    public event EventHandler<MushafPresenceState>? PresenceReceived;
    public event EventHandler<PeerRepeatRequest>? RepeatRequested;

    public Task SendPresenceAsync(MushafPresenceState state, CancellationToken cancellationToken = default)
    {
        PresenceReceived?.Invoke(this, state);
        return Task.CompletedTask;
    }

    public Task SendRepeatRequestAsync(PeerRepeatRequest request, CancellationToken cancellationToken = default)
    {
        RepeatRequested?.Invoke(this, request);
        return Task.CompletedTask;
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}