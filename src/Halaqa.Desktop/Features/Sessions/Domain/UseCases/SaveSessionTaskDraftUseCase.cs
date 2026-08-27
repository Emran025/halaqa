using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class SaveSessionTaskDraftUseCase
{
    private readonly ISessionTaskDirectoryRepository repository;

    public SaveSessionTaskDraftUseCase(ISessionTaskDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionTaskListItem>> ExecuteAsync(SaveSessionTaskDraftCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = SessionTaskCommandValidation.Validate(command);
        return validationError is null
            ? repository.SaveDraftAsync(command, cancellationToken)
            : Task.FromResult(Result<SessionTaskListItem>.Failure(validationError));
    }
}
