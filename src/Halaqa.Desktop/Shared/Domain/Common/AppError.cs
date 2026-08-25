namespace Halaqa.Desktop.Shared.Domain.Common;

public enum AppErrorKind
{
    Network,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Validation,
    Server,
    Unknown
}

public sealed record FieldError(string Field, IReadOnlyList<string> Messages);

public sealed record AppError(
    AppErrorKind Kind,
    string Message,
    IReadOnlyList<FieldError>? FieldErrors = null,
    string? Code = null)
{
    public static AppError Network(string message) => new(AppErrorKind.Network, message);
    public static AppError Unauthorized(string message) => new(AppErrorKind.Unauthorized, message);
    public static AppError Validation(string message, IReadOnlyList<FieldError> fieldErrors) =>
        new(AppErrorKind.Validation, message, fieldErrors);
}
