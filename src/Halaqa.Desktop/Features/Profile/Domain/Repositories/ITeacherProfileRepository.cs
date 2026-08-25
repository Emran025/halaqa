using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Domain.Repositories;

public interface ITeacherProfileRepository
{
    Task<Result<TeacherProfile>> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<Result<TeacherProfile>> UpdateCurrentAsync(
        UpdateTeacherProfileCommand command,
        CancellationToken cancellationToken = default);
}
