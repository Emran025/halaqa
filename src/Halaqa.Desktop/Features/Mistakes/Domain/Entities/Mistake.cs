namespace Halaqa.Desktop.Features.Mistakes.Domain.Entities;

public enum MistakeType
{
    None,
    Memory,
    Grammar,
    Pronunciation,
    Timing
}

public enum MistakeSyncState
{
    Pending,
    Synced,
    Conflict,
    Failed
}

public sealed record MistakeDraft(
    Guid SessionId,
    Guid TaskId,
    int AyahId,
    int? PageNumber,
    int WordIndex,
    MistakeType MistakeType,
    string? Note,
    Guid ClientOperationId);

public sealed record PendingMistakeOperation(
    Guid LocalId,
    MistakeDraft Draft,
    MistakeSyncState SyncState,
    DateTimeOffset CreatedAt,
    string? FailureReason = null);
