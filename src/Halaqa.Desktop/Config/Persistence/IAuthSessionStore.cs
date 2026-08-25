using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Halaqa.Desktop.Config.Persistence;

public sealed record AuthSession(
    string UserId,
    string Role,
    string Name,
    string Email,
    string AccessToken,
    DateTimeOffset ExpiresAt);

public interface IAuthSessionStore
{
    Task<AuthSession?> ReadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

public sealed class WindowsProtectedAuthSessionStore : IAuthSessionStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Halaqa.Desktop.AuthSession.v1");
    private readonly string _sessionPath;

    public WindowsProtectedAuthSessionStore()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Halaqa");
        Directory.CreateDirectory(appData);
        _sessionPath = Path.Combine(appData, "auth.session");
    }

    public async Task<AuthSession?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_sessionPath))
        {
            return null;
        }

        try
        {
            var protectedBytes = await File.ReadAllBytesAsync(_sessionPath, cancellationToken);
            var jsonBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
            return JsonSerializer.Deserialize<AuthSession>(jsonBytes);
        }
        catch (CryptographicException)
        {
            await ClearAsync(cancellationToken);
            return null;
        }
        catch (JsonException)
        {
            await ClearAsync(cancellationToken);
            return null;
        }
    }

    public async Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(session);
        var protectedBytes = ProtectedData.Protect(jsonBytes, Entropy, DataProtectionScope.CurrentUser);
        var temporaryPath = $"{_sessionPath}.tmp";

        await File.WriteAllBytesAsync(temporaryPath, protectedBytes, cancellationToken);
        File.Move(temporaryPath, _sessionPath, overwrite: true);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_sessionPath))
        {
            File.Delete(_sessionPath);
        }

        return Task.CompletedTask;
    }
}
