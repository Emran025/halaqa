using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class ListSessionTasksUseCase
{
    private readonly ISessionTaskDirectoryRepository repository;

    public ListSessionTasksUseCase(ISessionTaskDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionTaskPage>> ExecuteAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        sessionId == Guid.Empty
            ? Task.FromResult(Result<SessionTaskPage>.Failure(new AppError(AppErrorKind.Validation, "معرّف الجلسة غير صالح.")))
            : repository.ListAsync(sessionId, cancellationToken);
}
