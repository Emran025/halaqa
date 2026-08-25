using Halaqa.Desktop.Config.Connectivity;
using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Config.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Halaqa.Desktop.Config;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHalaqaInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAuthSessionStore, WindowsProtectedAuthSessionStore>();
        services.AddSingleton<IConnectivityService, NetworkConnectivityService>();
        services.AddTransient<BearerTokenHandler>();
        services.AddHttpClient<IApiClient, ApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ApiOptions>>().Value;
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException("يجب ضبط Api:BaseUrl بعنوان HTTPS صالح للخادم.");
            }

            client.BaseAddress = baseUri;
            client.Timeout = options.RequestTimeout;
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }).AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
