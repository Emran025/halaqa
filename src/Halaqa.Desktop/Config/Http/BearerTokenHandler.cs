using Halaqa.Desktop.Config.Persistence;

namespace Halaqa.Desktop.Config.Http;

public sealed class BearerTokenHandler(IAuthSessionStore sessionStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var session = await sessionStore.ReadAsync(cancellationToken);
        if (session is not null && session.ExpiresAt > DateTimeOffset.UtcNow)
        {
            request.Headers.Authorization = new("Bearer", session.AccessToken);
        }

        request.Headers.TryAddWithoutValidation("X-Client-Request-Id", Guid.NewGuid().ToString());
        return await base.SendAsync(request, cancellationToken);
    }
}
