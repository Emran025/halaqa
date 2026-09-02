using Halaqa.Desktop.Features.Evaluations.Domain.Entities;
using Halaqa.Desktop.Features.Evaluations.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Evaluations.Domain.UseCases;

public sealed class GetTaskEvaluationsUseCase
{
    private readonly ITaskEvaluationRepository repository;

    public GetTaskEvaluationsUseCase(ITaskEvaluationRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<TaskEvaluationSummary>> ExecuteAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default) =>
        sessionId == Guid.Empty || taskId == Guid.Empty
            ? Task.FromResult(Result<TaskEvaluationSummary>.Failure(new AppError(AppErrorKind.Validation, "معرّف الجلسة أو المهمة غير صالح.")))
            : repository.GetAsync(sessionId, taskId, cancellationToken);
}

public sealed class UpsertTaskEvaluationUseCase
{
    private readonly ITaskEvaluationRepository repository;

    public UpsertTaskEvaluationUseCase(ITaskEvaluationRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<TaskEvaluationSummary>> ExecuteAsync(UpsertTaskEvaluationCommand command, CancellationToken cancellationToken = default)
    {
        var error = Validate(command);
        return error is null
            ? repository.UpsertAsync(command, cancellationToken)
            : Task.FromResult(Result<TaskEvaluationSummary>.Failure(error));
    }

    private static AppError? Validate(UpsertTaskEvaluationCommand command)
    {
        if (command.SessionId == Guid.Empty || command.TaskId == Guid.Empty)
        {
            return new AppError(AppErrorKind.Validation, "معرّف الجلسة أو المهمة غير صالح.");
        }
        if (command.Score is < 0 or > 100)
        {
            return new AppError(AppErrorKind.Validation, "الدرجة يجب أن تكون بين 0 و100.");
        }
        if (command.Comment?.Length > 2000)
        {
            return new AppError(AppErrorKind.Validation, "الملاحظة يجب ألا تتجاوز 2000 حرف.");
        }

        return null;
    }
}
