using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Quran.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Data.DataSources.Remote;

internal interface IQuranRemoteDataSource
{
    Task<Result<QuranPageResponseDto>> GetPageAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default);
}

internal sealed class QuranRemoteDataSource : IQuranRemoteDataSource
{

    private readonly IApiClient apiClient;


    public QuranRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<QuranPageResponseDto>> GetPageAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<QuranPageResponseDto>($"quran/pages/{pageNumber}?edition_id={editionId}", cancellationToken);
}
