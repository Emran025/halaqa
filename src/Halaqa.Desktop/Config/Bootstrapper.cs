using Halaqa.Desktop.Features.Auth;
using Halaqa.Desktop.Features.Evaluations;
using Halaqa.Desktop.Features.FollowUp;
using Halaqa.Desktop.Features.Halaqas;
using Halaqa.Desktop.Features.Memberships;
using Halaqa.Desktop.Features.Mistakes;
using Halaqa.Desktop.Features.Notifications;
using Halaqa.Desktop.Features.Notes;
using Halaqa.Desktop.Features.Profile;
using Halaqa.Desktop.Features.Progress;
using Halaqa.Desktop.Features.Registrations;
using Halaqa.Desktop.Features.Quran;
using Halaqa.Desktop.Features.Sessions;
using Halaqa.Desktop.Features.TeacherDocuments;
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
        services.AddProfileFeature();
        services.AddTeacherDocumentsFeature();
        services.AddHalaqasFeature();
        services.AddMembershipsFeature();
        services.AddRegistrationsFeature();
        services.AddFollowUpFeature();
        services.AddQuranFeature();
        services.AddMistakesFeature();
        services.AddEvaluationsFeature();
        services.AddNotificationsFeature();
        services.AddNotesFeature();
        services.AddProgressFeature();
        services.AddSessionsFeature();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainShellViewModel>();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
