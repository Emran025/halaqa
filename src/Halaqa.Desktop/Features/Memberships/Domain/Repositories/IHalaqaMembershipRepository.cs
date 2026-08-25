using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Memberships.Domain.Repositories;

public interface IHalaqaMembershipRepository
{
    Task<Result<MembershipPage>> ListAsync(
        Guid halaqaId,
        string? status = null,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<Result<HalaqaMembership>> AssignAsync(
        AssignStudentToHalaqaCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<HalaqaMembership>> UpdateAsync(
        UpdateHalaqaMembershipCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(
        Guid halaqaId,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}
