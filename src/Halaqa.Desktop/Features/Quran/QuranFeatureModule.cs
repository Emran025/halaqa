using Halaqa.Desktop.Features.Quran.Data.DataSources.Local;
using Halaqa.Desktop.Features.Quran.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Quran.Data.Repositories;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Quran;

public static class QuranFeatureModule
{
    public static IServiceCollection AddQuranFeature(this IServiceCollection services)
    {
        services.AddSingleton<IQuranRemoteDataSource, QuranRemoteDataSource>();
        services.AddSingleton<IQuranPageCache, FileQuranPageCache>();
        services.AddSingleton<IQuranRepository, QuranRepository>();
        services.AddSingleton<GetQuranPageUseCase>();
        return services;
    }
}
