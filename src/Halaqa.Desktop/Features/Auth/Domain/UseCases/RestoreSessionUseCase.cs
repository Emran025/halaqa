using Halaqa.Desktop.Config.Persistence;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Time;

namespace Halaqa.Desktop.Features.Auth.Domain.UseCases;

public sealed class RestoreSessionUseCase(IAuthSessionStore sessionStore, IClock clock)
{
    public async Task<AuthenticatedUser?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessionStore.ReadAsync(cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt <= clock.UtcNow || !TryParseRole(session.Role, out var role) || string.IsNullOrWhiteSpace(session.AccessToken))
        {
            await sessionStore.ClearAsync(cancellationToken);
            return null;
        }

        if (!Guid.TryParse(session.UserId, out var userId))
        {
            await sessionStore.ClearAsync(cancellationToken);
            return null;
        }

        return new AuthenticatedUser(
            new AuthUser(userId, role, session.Name, session.Email, "active"),
            session.AccessToken,
            session.ExpiresAt);
    }

    private static bool TryParseRole(string value, out UserRole role) =>
        Enum.TryParse(value, ignoreCase: true, out role);
}
