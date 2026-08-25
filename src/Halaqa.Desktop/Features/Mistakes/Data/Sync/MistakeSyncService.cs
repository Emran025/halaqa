using Halaqa.Desktop.Config.Connectivity;
using Halaqa.Desktop.Features.Mistakes.Domain.Repositories;

namespace Halaqa.Desktop.Features.Mistakes.Data.Sync;

internal interface IMistakeSyncService
{
    Task SynchronizeAsync(CancellationToken cancellationToken = default);
}

internal sealed class MistakeSyncService : IMistakeSyncService, IDisposable
{
    private readonly IMistakeRepository _repository;
    private readonly IConnectivityService _connectivityService;
    private readonly SemaphoreSlim _syncGate = new(1, 1);

    public MistakeSyncService(
        IMistakeRepository repository,
        IConnectivityService connectivityService)
    {
        _repository = repository;
        _connectivityService = connectivityService;
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivityService.IsOnline || !await _syncGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            await _repository.SynchronizePendingAsync(cancellationToken);
        }
        finally
        {
            _syncGate.Release();
        }
    }

    public void Dispose()
    {
        _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        _syncGate.Dispose();
    }

    private async void OnConnectivityChanged(object? sender, bool isOnline)
    {
        if (isOnline)
        {
            await SynchronizeAsync();
        }
    }
}
