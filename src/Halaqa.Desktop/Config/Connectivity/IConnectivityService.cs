using System.Net.NetworkInformation;

namespace Halaqa.Desktop.Config.Connectivity;

public interface IConnectivityService
{
    bool IsOnline { get; }
    event EventHandler<bool>? ConnectivityChanged;
}

public sealed class NetworkConnectivityService : IConnectivityService, IDisposable
{
    public NetworkConnectivityService()
    {
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
    }

    public bool IsOnline => NetworkInterface.GetIsNetworkAvailable();

    public event EventHandler<bool>? ConnectivityChanged;

    public void Dispose()
    {
        NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs eventArgs) =>
        ConnectivityChanged?.Invoke(this, eventArgs.IsAvailable);
}
