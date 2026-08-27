namespace Halaqa.Desktop.Features.Sessions.Domain.Entities;

public sealed record CreateSessionTaskCommand(
    Guid SessionId,
    SessionTaskType TaskType,
    Guid ClientOperationId);
