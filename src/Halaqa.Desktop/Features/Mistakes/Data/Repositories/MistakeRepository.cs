using Halaqa.Desktop.Features.Mistakes.Data.DataSources.Local;
using Halaqa.Desktop.Features.Mistakes.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Halaqa.Desktop.Features.Mistakes.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Mistakes.Data.Repositories;

internal sealed class MistakeRepository : IMistakeRepository
{

    private readonly IMistakeOutbox outbox;

    private readonly IMistakeRemoteDataSource remoteDataSource;

    private readonly Halaqa.Desktop.Shared.Domain.Time.IClock clock;


    public MistakeRepository(

        IMistakeOutbox outbox,

        IMistakeRemoteDataSource remoteDataSource,

        Halaqa.Desktop.Shared.Domain.Time.IClock clock

    )

    {

        this.outbox = outbox;

        this.remoteDataSource = remoteDataSource;

        this.clock = clock;

    }

    public async Task<Result<PendingMistakeOperation>> QueueCreateAsync(
        MistakeDraft draft,
        CancellationToken cancellationToken = default)
    {
        var operation = new PendingMistakeOperation(
            Guid.NewGuid(),
            draft,
            MistakeSyncState.Pending,
            clock.UtcNow);
        await outbox.UpsertAsync(operation, cancellationToken);

        var synchronized = await TrySynchronizeAsync(operation, cancellationToken);
        await outbox.UpsertAsync(synchronized, cancellationToken);
        return Result<PendingMistakeOperation>.Success(synchronized);
    }

    public async Task<Result<IReadOnlyList<PendingMistakeOperation>>> SynchronizePendingAsync(
        CancellationToken cancellationToken = default)
    {
        var operations = await outbox.ReadAsync(cancellationToken);
        var updated = new List<PendingMistakeOperation>(operations.Count);

        foreach (var operation in operations.OrderBy(item => item.CreatedAt))
        {
            var current = operation.SyncState == MistakeSyncState.Pending
                ? await TrySynchronizeAsync(operation, cancellationToken)
                : operation;
            if (current != operation)
            {
                await outbox.UpsertAsync(current, cancellationToken);
            }

            updated.Add(current);
        }

        return Result<IReadOnlyList<PendingMistakeOperation>>.Success(updated);
    }

    private async Task<PendingMistakeOperation> TrySynchronizeAsync(
        PendingMistakeOperation operation,
        CancellationToken cancellationToken)
    {
        var result = await remoteDataSource.CreateAsync(operation.Draft, cancellationToken);
        if (result.IsSuccess)
        {
            return operation with { SyncState = MistakeSyncState.Synced, FailureReason = null };
        }

        var error = result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر مزامنة الخطأ.");
        return error.Kind switch
        {
            AppErrorKind.Network or AppErrorKind.Server => operation with
            {
                SyncState = MistakeSyncState.Pending,
                FailureReason = error.Message
            },
            AppErrorKind.Conflict => operation with
            {
                SyncState = MistakeSyncState.Conflict,
                FailureReason = error.Message
            },
            _ => operation with
            {
                SyncState = MistakeSyncState.Failed,
                FailureReason = error.Message
            }
        };
    }
}
