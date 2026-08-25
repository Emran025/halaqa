namespace Halaqa.Desktop.Shared.Domain.Common;

public sealed class Result
{
    private Result(bool isSuccess, AppError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public AppError? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(AppError error) => new(false, error);
}

public sealed class Result<T>
{
    private Result(T? value, AppError? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public AppError? Error { get; }

    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(AppError error) => new(default, error, false);
}
