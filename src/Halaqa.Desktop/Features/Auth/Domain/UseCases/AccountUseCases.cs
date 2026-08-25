using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Domain.UseCases;

public sealed class RegisterStudentUseCase(IAuthRepository repository)
{
    public Task<Result<AuthenticatedUser>> ExecuteAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateStudent(command);
        return validationError is null
            ? repository.RegisterStudentAsync(command, cancellationToken)
            : Task.FromResult(Result<AuthenticatedUser>.Failure(validationError));
    }

    private static AppError? ValidateStudent(StudentRegistrationCommand command)
    {
        if (command.ClientOperationId == Guid.Empty || string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Phone) || string.IsNullOrWhiteSpace(command.PhoneZone))
        {
            return new AppError(AppErrorKind.Validation, "يرجى إكمال البيانات الأساسية المطلوبة.");
        }
        if (command.Password.Length < 8 || command.Password != command.PasswordConfirmation)
        {
            return new AppError(AppErrorKind.Validation, "كلمة المرور وتأكيدها غير متطابقين أو أقصر من الحد الأدنى.");
        }
        if (command.AttendancePreferences.WeeklySlots.Count == 0 || command.FollowUpPlan.Details.Count == 0)
        {
            return new AppError(AppErrorKind.Validation, "يلزم إدخال وقت حضور واحد وتفصيل واحد على الأقل لخطة المتابعة.");
        }
        if (command.AttendancePreferences.WeeklySlots.Any(slot => slot.DayOfWeek is < 0 or > 6 || string.IsNullOrWhiteSpace(slot.From) || string.IsNullOrWhiteSpace(slot.To)))
        {
            return new AppError(AppErrorKind.Validation, "أحد أوقات الحضور غير صالح.");
        }
        return null;
    }
}

public sealed class RegisterTeacherUseCase(IAuthRepository repository)
{
    public Task<Result<AuthenticatedUser>> ExecuteAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ClientOperationId == Guid.Empty || string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Qualification) || command.ExperienceYears is < 0 or > 80)
        {
            return Task.FromResult(Result<AuthenticatedUser>.Failure(new AppError(AppErrorKind.Validation, "يرجى إكمال بيانات المعلم المطلوبة بصورة صحيحة.")));
        }
        if (command.Password.Length < 8 || command.Password != command.PasswordConfirmation)
        {
            return Task.FromResult(Result<AuthenticatedUser>.Failure(new AppError(AppErrorKind.Validation, "كلمة المرور وتأكيدها غير متطابقين أو أقصر من الحد الأدنى.")));
        }
        return repository.RegisterTeacherAsync(command, cancellationToken);
    }
}

public sealed class RequestPasswordResetUseCase(IAuthRepository repository)
{
    public Task<Result> ExecuteAsync(string email, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(email) || !email.Contains('@')
            ? Task.FromResult(Result.Failure(new AppError(AppErrorKind.Validation, "أدخل بريداً إلكترونياً صالحاً.")))
            : repository.RequestPasswordResetAsync(email, cancellationToken);
}
