using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Domain.Repositories;

public interface IStudentProfileRepository
{
    Task<Result<StudentProfile>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<StudentProfile>> UpdateCurrentAsync(
        UpdateStudentProfileCommand command,
        CancellationToken cancellationToken = default);
}
