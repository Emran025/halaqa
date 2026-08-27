using Halaqa.Desktop.Features.Progress.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Progress.Data.Repositories;
using Halaqa.Desktop.Features.Progress.Domain.Repositories;
using Halaqa.Desktop.Features.Progress.Domain.UseCases;
using Halaqa.Desktop.Features.Progress.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Progress;

public static class ProgressFeatureModule
{
    public static IServiceCollection AddProgressFeature(this IServiceCollection services)
    {
        services.AddSingleton<IStudentProgressRemoteDataSource, StudentProgressRemoteDataSource>();
        services.AddSingleton<IStudentProgressRepository, StudentProgressRepository>();
        services.AddSingleton<GetStudentProgressUseCase>();
        services.AddTransient<StudentProgressViewModel>();
        return services;
    }
}
