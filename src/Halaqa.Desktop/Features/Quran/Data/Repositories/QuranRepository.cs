using Halaqa.Desktop.Features.Quran.Data.DataSources.Local;
using Halaqa.Desktop.Features.Quran.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Quran.Data.Mappers;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Data.Repositories;

internal sealed class QuranRepository(
    IQuranRemoteDataSource remoteDataSource,
    IQuranPageCache localCache) : IQuranRepository
{
    public async Task<Result<QuranPage>> GetPageAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default)
    {
        var remote = await remoteDataSource.GetPageAsync(editionId, pageNumber, cancellationToken);
        if (remote.IsSuccess && remote.Value is not null)
        {
            await localCache.SaveAsync(remote.Value.QuranPage, cancellationToken);
            return Result<QuranPage>.Success(QuranMapper.ToDomain(remote.Value.QuranPage, isFromLocalCache: false));
        }

        var cached = await localCache.ReadAsync(editionId, pageNumber, cancellationToken);
        if (cached is not null)
        {
            return Result<QuranPage>.Success(QuranMapper.ToDomain(cached, isFromLocalCache: true));
        }

        return Result<QuranPage>.Failure(remote.Error ?? AppError.Network("لا توجد نسخة محلية من هذه الصفحة."));
    }
}
