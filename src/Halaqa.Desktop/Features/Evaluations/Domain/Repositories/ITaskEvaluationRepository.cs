using Halaqa.Desktop.Features.Evaluations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Evaluations.Domain.Repositories;

public interface ITaskEvaluationRepository
{
    Task<Result<TaskEvaluationSummary>> GetAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default);

    Task<Result<TaskEvaluationSummary>> UpsertAsync(UpsertTaskEvaluationCommand command, CancellationToken cancellationToken = default);
}
