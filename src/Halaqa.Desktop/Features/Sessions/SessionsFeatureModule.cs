using Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Sessions.Data.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Sessions;

public static class SessionsFeatureModule
{
    public static IServiceCollection AddSessionsFeature(this IServiceCollection services)
    {
        services.AddSingleton<ILiveSessionRemoteDataSource, LiveSessionRemoteDataSource>();
        services.AddSingleton<ILiveSessionRepository, LiveSessionRepository>();
        services.AddSingleton<PrepareLiveSessionUseCase>();
        services.AddSingleton<SaveOfficialMushafStateUseCase>();
        services.AddTransient<LiveSessionStore>();
        return services;
    }
}
