using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Domain.Repositories;

public interface IProfileRepository
{
    Task<Result<UserProfile>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<UserProfile>> UpdateCurrentAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default);
}
