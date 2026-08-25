using Halaqa.Desktop.Features.Mistakes.Data.DataSources.Local;
using Halaqa.Desktop.Features.Mistakes.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Mistakes.Data.Repositories;
using Halaqa.Desktop.Features.Mistakes.Domain.Repositories;
using Halaqa.Desktop.Features.Mistakes.Domain.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Mistakes;

public static class MistakesFeatureModule
{
    public static IServiceCollection AddMistakesFeature(this IServiceCollection services)
    {
        services.AddSingleton<IMistakeOutbox, FileMistakeOutbox>();
        services.AddSingleton<IMistakeRemoteDataSource, MistakeRemoteDataSource>();
        services.AddSingleton<IMistakeRepository, MistakeRepository>();
        services.AddSingleton<QueueMistakeUseCase>();
        return services;
    }
}
