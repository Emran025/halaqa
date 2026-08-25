using Halaqa.Desktop.Features.Registrations.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Registrations.Data.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Registrations;

public static class RegistrationsFeatureModule
{
    public static IServiceCollection AddRegistrationsFeature(this IServiceCollection services)
    {
        services.AddSingleton<IRegistrationRequestRemoteDataSource, RegistrationRequestRemoteDataSource>();
        services.AddSingleton<IRegistrationRequestRepository, RegistrationRequestRepository>();
        services.AddSingleton<ListHalaqaRegistrationRequestsUseCase>();
        services.AddSingleton<AcceptRegistrationRequestUseCase>();
        services.AddSingleton<RejectRegistrationRequestUseCase>();
        services.AddSingleton<RequestRegistrationCompletionUseCase>();
        services.AddSingleton<HalaqaRegistrationRequestsViewModel>();
        return services;
    }
}
