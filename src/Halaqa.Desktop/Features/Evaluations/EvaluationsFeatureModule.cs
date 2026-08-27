using Halaqa.Desktop.Features.Evaluations.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Evaluations.Data.Repositories;
using Halaqa.Desktop.Features.Evaluations.Domain.Repositories;
using Halaqa.Desktop.Features.Evaluations.Domain.UseCases;
using Halaqa.Desktop.Features.Evaluations.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Evaluations;

public static class EvaluationsFeatureModule
{
    public static IServiceCollection AddEvaluationsFeature(this IServiceCollection services)
    {
        services.AddSingleton<ITaskEvaluationRemoteDataSource, TaskEvaluationRemoteDataSource>();
        services.AddSingleton<ITaskEvaluationRepository, TaskEvaluationRepository>();
        services.AddSingleton<GetTaskEvaluationsUseCase>();
        services.AddSingleton<UpsertTaskEvaluationUseCase>();
        services.AddTransient<TaskEvaluationViewModel>();
        return services;
    }
}
