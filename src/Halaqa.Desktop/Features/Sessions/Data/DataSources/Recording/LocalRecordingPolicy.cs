using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Recording;

internal static class LocalRecordingPolicy
{
    public static Result<string> CreateOutputPath(Guid sessionId, string outputDirectory, DateTimeOffset startedAt)
    {
        if (sessionId == Guid.Empty || string.IsNullOrWhiteSpace(outputDirectory))
        {
            return Result<string>.Failure(new AppError(
                AppErrorKind.Validation,
                "يلزم اختيار مجلد صالح قبل بدء التسجيل المحلي."));
        }

        try
        {
            Directory.CreateDirectory(outputDirectory);
            var fileName = $"halaqa-{sessionId:N}-{startedAt:yyyyMMdd-HHmmss}.mp4";
            return Result<string>.Success(Path.Combine(outputDirectory, fileName));
        }
        catch (UnauthorizedAccessException)
        {
            return Result<string>.Failure(new AppError(
                AppErrorKind.Forbidden,
                "لا يملك التطبيق صلاحية الكتابة في مجلد التسجيل المختار."));
        }
        catch (IOException)
        {
            return Result<string>.Failure(new AppError(
                AppErrorKind.Data,
                "تعذر تجهيز ملف التسجيل المحلي."));
        }
    }
}
