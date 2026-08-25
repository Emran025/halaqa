using Halaqa.Desktop.Features.Auth;
using Halaqa.Desktop.Features.Mistakes;
using Halaqa.Desktop.Features.Quran;
using Halaqa.Desktop.Features.Sessions;
using Halaqa.Desktop.Presentation;
using Halaqa.Desktop.Shared.Domain.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Config;

public static class Bootstrapper
{
    public static ServiceProvider BuildServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables(prefix: "HALAQA_")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));
        services.AddSingleton<IClock, SystemClock>();

        services.AddHalaqaInfrastructure();
        services.AddAuthFeature();
        services.AddQuranFeature();
        services.AddMistakesFeature();
        services.AddSessionsFeature();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainShellViewModel>();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
