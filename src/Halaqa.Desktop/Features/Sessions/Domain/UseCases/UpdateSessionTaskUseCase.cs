using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class UpdateSessionTaskUseCase
{
    private readonly ISessionTaskDirectoryRepository repository;

    public UpdateSessionTaskUseCase(ISessionTaskDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionTaskListItem>> ExecuteAsync(UpdateSessionTaskCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = SessionTaskCommandValidation.Validate(command);
        return validationError is null
            ? repository.UpdateAsync(command, cancellationToken)
            : Task.FromResult(Result<SessionTaskListItem>.Failure(validationError));
    }
}
