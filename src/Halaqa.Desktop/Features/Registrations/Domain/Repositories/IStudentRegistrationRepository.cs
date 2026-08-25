using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Domain.Repositories;

public interface IStudentRegistrationRepository
{
    Task<Result<AvailableTeacherPage>> ListAvailableTeachersAsync(
        string? code = null,
        string? search = null,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<Result<AvailableTeacher>> GetPublicTeacherAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationRequest>> CreateAsync(
        CreateStudentRegistrationRequestCommand command,
        CancellationToken cancellationToken = default);
}
