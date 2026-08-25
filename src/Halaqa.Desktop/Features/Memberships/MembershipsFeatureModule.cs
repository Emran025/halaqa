using Halaqa.Desktop.Features.Memberships.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Memberships.Data.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Memberships;

public static class MembershipsFeatureModule
{
    public static IServiceCollection AddMembershipsFeature(this IServiceCollection services)
    {
        services.AddSingleton<IHalaqaMembershipRemoteDataSource, HalaqaMembershipRemoteDataSource>();
        services.AddSingleton<IHalaqaMembershipRepository, HalaqaMembershipRepository>();
        services.AddSingleton<ListHalaqaMembershipsUseCase>();
        services.AddSingleton<AssignStudentToHalaqaUseCase>();
        services.AddSingleton<UpdateHalaqaMembershipUseCase>();
        services.AddSingleton<RemoveHalaqaMembershipUseCase>();
        services.AddSingleton<HalaqaMembershipsViewModel>();
        return services;
    }
}
