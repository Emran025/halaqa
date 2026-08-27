using Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Sessions.Data.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.Stores;
using Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Sessions;

public static class SessionsFeatureModule
{
    public static IServiceCollection AddSessionsFeature(this IServiceCollection services)
    {
        services.AddSingleton<ILiveSessionRemoteDataSource, LiveSessionRemoteDataSource>();
        services.AddSingleton<ISessionDirectoryRemoteDataSource, SessionDirectoryRemoteDataSource>();
        services.AddSingleton<ISessionTaskDirectoryRemoteDataSource, SessionTaskDirectoryRemoteDataSource>();
        services.AddSingleton<ILiveSessionRepository, LiveSessionRepository>();
        services.AddSingleton<ISessionDirectoryRepository, SessionDirectoryRepository>();
        services.AddSingleton<ISessionTaskDirectoryRepository, SessionTaskDirectoryRepository>();
        services.AddSingleton<PrepareLiveSessionUseCase>();
        services.AddSingleton<SaveOfficialMushafStateUseCase>();
        services.AddSingleton<ListSessionsUseCase>();
        services.AddSingleton<ListSessionTasksUseCase>();
        services.AddTransient<LiveSessionStore>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<SessionTasksViewModel>();
        return services;
    }
}
