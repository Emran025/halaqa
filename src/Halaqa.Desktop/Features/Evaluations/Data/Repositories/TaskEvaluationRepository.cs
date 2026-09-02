using Halaqa.Desktop.Features.Evaluations.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Evaluations.Data.Mappers;
using Halaqa.Desktop.Features.Evaluations.Domain.Entities;
using Halaqa.Desktop.Features.Evaluations.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Evaluations.Data.Repositories;

internal sealed class TaskEvaluationRepository : ITaskEvaluationRepository
{
    private readonly ITaskEvaluationRemoteDataSource remoteDataSource;

    public TaskEvaluationRepository(ITaskEvaluationRemoteDataSource remoteDataSource)
    {
        this.remoteDataSource = remoteDataSource;
    }

    public async Task<Result<TaskEvaluationSummary>> GetAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.GetAsync(sessionId, taskId, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<TaskEvaluationSummary>.Success(TaskEvaluationMapper.ToDomain(result.Value))
            : Result<TaskEvaluationSummary>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحميل تقييمات المهمة."));
    }

    public async Task<Result<TaskEvaluationSummary>> UpsertAsync(UpsertTaskEvaluationCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpsertAsync(command, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<TaskEvaluationSummary>.Success(TaskEvaluationMapper.ToDomain(result.Value))
            : Result<TaskEvaluationSummary>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر حفظ تقييم المهمة."));
    }
}
