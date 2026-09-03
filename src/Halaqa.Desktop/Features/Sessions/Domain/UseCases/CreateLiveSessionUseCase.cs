using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class CreateLiveSessionUseCase
{
    private readonly ISessionDirectoryRepository repository;

    public CreateLiveSessionUseCase(ISessionDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionListItem>> ExecuteAsync(
        CreateLiveSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.HalaqaId == Guid.Empty || command.StudentId == Guid.Empty || command.ClientOperationId == Guid.Empty)
        {
            return Task.FromResult(Result<SessionListItem>.Failure(
                new AppError(AppErrorKind.Validation, "معرّف الحلقة أو الطالب أو العملية غير صالح.")));
        }

        return repository.CreateAsync(command, cancellationToken);
    }
}
