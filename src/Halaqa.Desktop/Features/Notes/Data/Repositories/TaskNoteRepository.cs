using Halaqa.Desktop.Features.Notes.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Notes.Data.Mappers;
using Halaqa.Desktop.Features.Notes.Domain.Entities;
using Halaqa.Desktop.Features.Notes.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notes.Data.Repositories;

internal sealed class TaskNoteRepository : ITaskNoteRepository
{
    private readonly ITaskNoteRemoteDataSource remoteDataSource;

    public TaskNoteRepository(ITaskNoteRemoteDataSource remoteDataSource)
    {
        this.remoteDataSource = remoteDataSource;
    }

    public async Task<Result<TaskNotePage>> ListAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ListAsync(sessionId, taskId, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<TaskNotePage>.Success(TaskNoteMapper.ToDomain(result.Value))
            : Result<TaskNotePage>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحميل ملاحظات المهمة."));
    }

    public async Task<Result<TaskNote>> CreateAsync(CreateTaskNoteCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.CreateAsync(command, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<TaskNote>.Success(TaskNoteMapper.ToDomain(result.Value.Note))
            : Result<TaskNote>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر إنشاء الملاحظة."));
    }

    public async Task<Result<TaskNote>> UpdateAsync(UpdateTaskNoteCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateAsync(command, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<TaskNote>.Success(TaskNoteMapper.ToDomain(result.Value.Note))
            : Result<TaskNote>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحديث الملاحظة."));
    }

    public async Task<Result> DeleteAsync(DeleteTaskNoteCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.DeleteAsync(command, cancellationToken);
        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر حذف الملاحظة."));
    }
}
