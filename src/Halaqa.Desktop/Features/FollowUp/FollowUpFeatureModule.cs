using Halaqa.Desktop.Features.FollowUp.Data.DataSources.Remote;
using Halaqa.Desktop.Features.FollowUp.Data.Repositories;
using Halaqa.Desktop.Features.FollowUp.Domain.Repositories;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.FollowUp;

public static class FollowUpFeatureModule
{
    public static IServiceCollection AddFollowUpFeature(this IServiceCollection services)
    {
        services.AddSingleton<IFollowUpRemoteDataSource, FollowUpRemoteDataSource>();
        services.AddSingleton<IFollowUpRepository, FollowUpRepository>();
        services.AddSingleton<GetFollowUpPlanUseCase>();
        services.AddSingleton<UpdateFollowUpPlanUseCase>();
        services.AddSingleton<GetAvailabilityUseCase>();
        services.AddSingleton<UpdateAvailabilityUseCase>();
        services.AddSingleton<ListFollowUpItemsUseCase>();
        services.AddSingleton<CompleteFollowUpItemUseCase>();
        services.AddSingleton<SkipFollowUpItemUseCase>();
        services.AddSingleton<RescheduleFollowUpItemUseCase>();
        services.AddSingleton<ListStudentTrackingsUseCase>();
        services.AddSingleton<FollowUpViewModel>();
        return services;
    }
}
