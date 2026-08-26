using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Domain.UseCases;

public sealed class RegisterStudentUseCase
{

    private readonly IAuthRepository repository;


    public RegisterStudentUseCase(

        IAuthRepository repository

    )

    {

        this.repository = repository;

    }

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

public sealed class RegisterTeacherUseCase
{

    private readonly IAuthRepository repository;


    public RegisterTeacherUseCase(

        IAuthRepository repository

    )

    {

        this.repository = repository;

    }

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

public sealed class RequestPasswordResetUseCase
{

    private readonly IAuthRepository repository;


    public RequestPasswordResetUseCase(

        IAuthRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(string email, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(email) || !email.Contains('@')
            ? Task.FromResult(Result.Failure(new AppError(AppErrorKind.Validation, "أدخل بريداً إلكترونياً صالحاً.")))
            : repository.RequestPasswordResetAsync(email, cancellationToken);
}

public sealed class ResetPasswordUseCase
{

    private readonly IAuthRepository repository;


    public ResetPasswordUseCase(

        IAuthRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePasswordReset(email, token, password, passwordConfirmation);
        return validationError is null
            ? repository.ResetPasswordAsync(email, token, password, passwordConfirmation, cancellationToken)
            : Task.FromResult(Result.Failure(validationError));
    }

    private static AppError? ValidatePasswordReset(string email, string token, string password, string passwordConfirmation) =>
        string.IsNullOrWhiteSpace(email) || !email.Contains('@') || string.IsNullOrWhiteSpace(token)
            ? new AppError(AppErrorKind.Validation, "أدخل البريد الإلكتروني ورمز إعادة التعيين.")
            : ValidateNewPassword(password, passwordConfirmation);

    internal static AppError? ValidateNewPassword(string password, string passwordConfirmation) =>
        password.Length < 8 || password != passwordConfirmation
            ? new AppError(AppErrorKind.Validation, "كلمة المرور وتأكيدها غير متطابقين أو أقصر من ثمانية أحرف.")
            : null;
}

public sealed class ChangePasswordUseCase
{

    private readonly IAuthRepository repository;


    public ChangePasswordUseCase(

        IAuthRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(string currentPassword, string password, string passwordConfirmation, CancellationToken cancellationToken = default)
    {
        var validationError = string.IsNullOrWhiteSpace(currentPassword)
            ? new AppError(AppErrorKind.Validation, "أدخل كلمة المرور الحالية.")
            : ResetPasswordUseCase.ValidateNewPassword(password, passwordConfirmation);
        return validationError is null
            ? repository.ChangePasswordAsync(currentPassword, password, passwordConfirmation, cancellationToken)
            : Task.FromResult(Result.Failure(validationError));
    }
}
