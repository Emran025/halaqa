using Halaqa.Desktop.Features.Quran.Data.DataSources.Local;
using Halaqa.Desktop.Features.Quran.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Quran.Data.Mappers;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Data.Repositories;

internal sealed class QuranRepository : IQuranRepository
{

    private readonly IQuranLocalDataSource localDataSource;

    private readonly IQuranRemoteDataSource remoteDataSource;


    public QuranRepository(

        IQuranLocalDataSource localDataSource,

        IQuranRemoteDataSource remoteDataSource

    )

    {

        this.localDataSource = localDataSource;

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<QuranPage>> GetPageAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default)
    {
        var local = await localDataSource.GetPageAsync(editionId, pageNumber, cancellationToken);
        if (local.IsSuccess)
        {
            return local;
        }

        var remote = await remoteDataSource.GetPageAsync(editionId, pageNumber, cancellationToken);
        if (remote.IsSuccess && remote.Value is not null)
        {
            return Result<QuranPage>.Success(QuranMapper.ToDomain(remote.Value.QuranPage, isFromLocalCache: false));
        }

        return Result<QuranPage>.Failure(local.Error ?? remote.Error ?? new AppError(
            AppErrorKind.Cache,
            "تعذر تحميل صفحة المصحف من SQLite المحلي أو من الخدمة."));
    }
}
