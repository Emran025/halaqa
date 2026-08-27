using Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Sessions.Data.Mappers;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.Repositories;

internal sealed class SessionTaskDirectoryRepository : ISessionTaskDirectoryRepository
{
    private readonly ISessionTaskDirectoryRemoteDataSource remoteDataSource;

    public SessionTaskDirectoryRepository(ISessionTaskDirectoryRemoteDataSource remoteDataSource)
    {
        this.remoteDataSource = remoteDataSource;
    }

    public async Task<Result<SessionTaskPage>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ListAsync(sessionId, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<SessionTaskPage>.Success(SessionTaskDirectoryMapper.ToDomain(result.Value))
            : Result<SessionTaskPage>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحميل مهام الجلسة."));
    }

    public async Task<Result<SessionTaskListItem>> CreateAsync(CreateSessionTaskCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.CreateAsync(command, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<SessionTaskListItem>.Success(SessionTaskDirectoryMapper.ToDomain(result.Value.Task))
            : Result<SessionTaskListItem>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر إنشاء مهمة الجلسة."));
    }

    public async Task<Result<SessionTaskListItem>> SaveDraftAsync(SaveSessionTaskDraftCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.SaveDraftAsync(command, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<SessionTaskListItem>.Success(SessionTaskDirectoryMapper.ToDomain(result.Value.Task))
            : Result<SessionTaskListItem>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر حفظ مسودة المهمة."));
    }

    public async Task<Result<SessionTaskListItem>> UpdateAsync(UpdateSessionTaskCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateAsync(command, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<SessionTaskListItem>.Success(SessionTaskDirectoryMapper.ToDomain(result.Value.Task))
            : Result<SessionTaskListItem>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحديث مهمة الجلسة."));
    }
}
