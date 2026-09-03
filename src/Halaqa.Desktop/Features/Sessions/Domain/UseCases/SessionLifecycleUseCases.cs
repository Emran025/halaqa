using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class AcceptLiveSessionUseCase
{
    private readonly ISessionDirectoryRepository repository;

    public AcceptLiveSessionUseCase(ISessionDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionListItem>> ExecuteAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        sessionId == Guid.Empty
            ? Task.FromResult(Result<SessionListItem>.Failure(new AppError(AppErrorKind.Validation, "معرّف الجلسة غير صالح.")))
            : repository.AcceptAsync(sessionId, cancellationToken);
}

public sealed class RejectLiveSessionUseCase
{
    private readonly ISessionDirectoryRepository repository;

    public RejectLiveSessionUseCase(ISessionDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionListItem>> ExecuteAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        sessionId == Guid.Empty
            ? Task.FromResult(Result<SessionListItem>.Failure(new AppError(AppErrorKind.Validation, "معرّف الجلسة غير صالح.")))
            : repository.RejectAsync(sessionId, cancellationToken);
}
