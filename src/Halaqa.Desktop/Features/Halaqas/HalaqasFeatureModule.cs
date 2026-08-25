using Halaqa.Desktop.Features.Halaqas.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Halaqas.Data.Repositories;
using Halaqa.Desktop.Features.Halaqas.Domain.Repositories;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Halaqas.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Halaqas;

public static class HalaqasFeatureModule
{
    public static IServiceCollection AddHalaqasFeature(this IServiceCollection services)
    {
        services.AddSingleton<IHalaqaRemoteDataSource, HalaqaRemoteDataSource>();
        services.AddSingleton<IHalaqaRepository, HalaqaRepository>();
        services.AddSingleton<ListHalaqasUseCase>();
        services.AddSingleton<CreateHalaqaUseCase>();
        services.AddSingleton<UpdateHalaqaUseCase>();
        services.AddSingleton<ActivateHalaqaUseCase>();
        services.AddSingleton<DeactivateHalaqaUseCase>();
        services.AddSingleton<HalaqasViewModel>();
        return services;
    }
}
