using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Memberships.Domain.UseCases;

public sealed class ListHalaqaMembershipsUseCase(IHalaqaMembershipRepository repository)
{
    public Task<Result<MembershipPage>> ExecuteAsync(
        Guid halaqaId,
        string? status = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (halaqaId == Guid.Empty || page < 1)
        {
            return Task.FromResult(Result<MembershipPage>.Failure(new AppError(
                AppErrorKind.Validation,
                "معرّف الحلقة أو رقم الصفحة غير صالح.")));
        }

        return repository.ListAsync(halaqaId, status, page, cancellationToken);
    }
}

public sealed class AssignStudentToHalaqaUseCase(IHalaqaMembershipRepository repository)
{
    public Task<Result<HalaqaMembership>> ExecuteAsync(
        AssignStudentToHalaqaCommand command,
        CancellationToken cancellationToken = default) =>
        command.HalaqaId == Guid.Empty || command.StudentId == Guid.Empty
            ? Task.FromResult(Result<HalaqaMembership>.Failure(new AppError(
                AppErrorKind.Validation,
                "معرّف الحلقة أو الطالب غير صالح.")))
            : repository.AssignAsync(command, cancellationToken);
}

public sealed class UpdateHalaqaMembershipUseCase(IHalaqaMembershipRepository repository)
{
    public Task<Result<HalaqaMembership>> ExecuteAsync(
        UpdateHalaqaMembershipCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.HalaqaId == Guid.Empty || command.MembershipId == Guid.Empty)
        {
            return Task.FromResult(Result<HalaqaMembership>.Failure(new AppError(
                AppErrorKind.Validation,
                "معرّف الحلقة أو العضوية غير صالح.")));
        }
        if (command.Reason?.Trim().Length > 500)
        {
            return Task.FromResult(Result<HalaqaMembership>.Failure(new AppError(
                AppErrorKind.Validation,
                "سبب تغيير العضوية لا يتجاوز 500 حرف.")));
        }

        return repository.UpdateAsync(command, cancellationToken);
    }
}

public sealed class RemoveHalaqaMembershipUseCase(IHalaqaMembershipRepository repository)
{
    public Task<Result> ExecuteAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default) =>
        halaqaId == Guid.Empty || membershipId == Guid.Empty
            ? Task.FromResult(Result.Failure(new AppError(
                AppErrorKind.Validation,
                "معرّف الحلقة أو العضوية غير صالح.")))
            : repository.RemoveAsync(halaqaId, membershipId, cancellationToken);
}
