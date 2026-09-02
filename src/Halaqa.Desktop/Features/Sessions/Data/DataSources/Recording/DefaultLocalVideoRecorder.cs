using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Recording;

public sealed class DefaultLocalVideoRecorder : ILocalVideoRecorder
{
    public event EventHandler<LocalRecordingState>? StateChanged;

    public Task StartAsync(string outputDirectory, CancellationToken cancellationToken = default)
    {
        StateChanged?.Invoke(this, new LocalRecordingState(RecordingState.Recording, outputDirectory, TimeSpan.Zero, null));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        StateChanged?.Invoke(this, new LocalRecordingState(RecordingState.Idle, null, TimeSpan.Zero, null));
        return Task.CompletedTask;
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}