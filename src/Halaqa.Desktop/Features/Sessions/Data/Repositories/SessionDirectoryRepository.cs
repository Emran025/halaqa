using Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Sessions.Data.Mappers;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.Repositories;

internal sealed class SessionDirectoryRepository : ISessionDirectoryRepository
{
    private readonly ISessionDirectoryRemoteDataSource remoteDataSource;

    public SessionDirectoryRepository(ISessionDirectoryRemoteDataSource remoteDataSource)
    {
        this.remoteDataSource = remoteDataSource;
    }

    public async Task<Result<SessionPage>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ListAsync(query, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<SessionPage>.Success(SessionDirectoryMapper.ToDomain(result.Value))
            : Result<SessionPage>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحميل قائمة الجلسات."));
    }
}
