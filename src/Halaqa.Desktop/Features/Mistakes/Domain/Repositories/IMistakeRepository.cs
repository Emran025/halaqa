using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Mistakes.Domain.Repositories;

public interface IMistakeRepository
{
    Task<Result<PendingMistakeOperation>> QueueCreateAsync(MistakeDraft draft, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PendingMistakeOperation>>> SynchronizePendingAsync(CancellationToken cancellationToken = default);
}
