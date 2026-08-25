using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Domain.UseCases;

public sealed class GetCurrentProfileUseCase(IProfileRepository repository)
{
    public Task<Result<UserProfile>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        repository.GetCurrentAsync(cancellationToken);
}

public sealed class UpdateCurrentProfileUseCase(IProfileRepository repository)
{
    public Task<Result<UserProfile>> ExecuteAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.HasChanges)
        {
            return Task.FromResult(Result<UserProfile>.Failure(new AppError(AppErrorKind.Validation, "أدخل حقلاً واحداً على الأقل لتحديث الملف.")));
        }

        if (command.Name.IsSpecified &&
            (string.IsNullOrWhiteSpace(command.Name.Value) ||
             command.Name.Value.Trim().Length is < 2 or > 120))
        {
            return Task.FromResult(Result<UserProfile>.Failure(new AppError(AppErrorKind.Validation, "يجب أن يتكون الاسم من حرفين إلى 120 حرفاً.")));
        }

        return repository.UpdateCurrentAsync(command, cancellationToken);
    }
}
