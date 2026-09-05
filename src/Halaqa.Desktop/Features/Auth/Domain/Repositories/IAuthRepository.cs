using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Domain.Repositories;

public interface IAuthRepository
{
    Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthenticatedUser>> RegisterStudentAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuthenticatedUser>> RegisterTeacherAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default);
    Task<Result> ResendVerificationAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure(new AppError(AppErrorKind.Unknown, "إعادة تفعيل البريد غير متاحة في مصدر البيانات الحالي.")));
    Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(string currentPassword, string password, string passwordConfirmation, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
}
