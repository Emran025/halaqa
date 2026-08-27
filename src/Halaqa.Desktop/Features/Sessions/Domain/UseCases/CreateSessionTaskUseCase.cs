using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class CreateSessionTaskUseCase
{
    private readonly ISessionTaskDirectoryRepository repository;

    public CreateSessionTaskUseCase(ISessionTaskDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionTaskListItem>> ExecuteAsync(CreateSessionTaskCommand command, CancellationToken cancellationToken = default)
    {
        if (command.SessionId == Guid.Empty || command.ClientOperationId == Guid.Empty)
        {
            return Task.FromResult(Result<SessionTaskListItem>.Failure(new AppError(AppErrorKind.Validation, "تعذر تجهيز طلب المهمة.")));
        }

        return repository.CreateAsync(command, cancellationToken);
    }
}
