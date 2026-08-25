using Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Profile.Data.Repositories;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Features.Profile.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Profile;

public static class ProfileFeatureModule
{
    public static IServiceCollection AddProfileFeature(this IServiceCollection services)
    {
        services.AddSingleton<IProfileRemoteDataSource, ProfileRemoteDataSource>();
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        services.AddSingleton<GetCurrentProfileUseCase>();
        services.AddSingleton<UpdateCurrentProfileUseCase>();
        services.AddSingleton<GeneralProfileViewModel>();
        return services;
    }
}
