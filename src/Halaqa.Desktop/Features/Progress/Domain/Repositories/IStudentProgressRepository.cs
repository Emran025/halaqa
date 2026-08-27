using Halaqa.Desktop.Features.Progress.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Progress.Domain.Repositories;

public interface IStudentProgressRepository
{
    Task<Result<StudentProgress>> GetAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default);
}
