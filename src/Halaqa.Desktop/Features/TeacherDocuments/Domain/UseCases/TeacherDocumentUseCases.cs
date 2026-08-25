using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.TeacherDocuments.Domain.UseCases;

public sealed class ListTeacherDocumentsUseCase(ITeacherDocumentRepository repository)
{
    public Task<Result<TeacherDocumentPage>> ExecuteAsync(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return Task.FromResult(Result<TeacherDocumentPage>.Failure(new AppError(
                AppErrorKind.Validation,
                "رقم الصفحة يجب أن يكون واحداً أو أكبر.")));
        }

        return repository.ListAsync(page, cancellationToken);
    }
}

public sealed class CreateTeacherDocumentUseCase(ITeacherDocumentRepository repository)
{
    public Task<Result<TeacherDocument>> ExecuteAsync(
        CreateTeacherDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command);
        return validationError is null
            ? repository.CreateAsync(command, cancellationToken)
            : Task.FromResult(Result<TeacherDocument>.Failure(validationError));
    }

    private static AppError? Validate(CreateTeacherDocumentCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 250)
        {
            return new AppError(AppErrorKind.Validation, "اسم الوثيقة مطلوب ولا يتجاوز 250 حرفاً.");
        }

        if (string.IsNullOrWhiteSpace(command.CertificateType) || command.CertificateType.Trim().Length > 100)
        {
            return new AppError(AppErrorKind.Validation, "نوع الشهادة مطلوب ولا يتجاوز 100 حرفاً.");
        }

        var fields = new (string? Value, int Maximum, string Label)[]
        {
            (command.CertificateTypeOther, 150, "نوع الشهادة الآخر"),
            (command.Riwayah, 100, "الرواية"),
            (command.IssuingPlace, 200, "جهة الإصدار")
        };

        foreach (var field in fields)
        {
            if (field.Value?.Trim().Length > field.Maximum)
            {
                return new AppError(AppErrorKind.Validation, $"يجب ألا يتجاوز {field.Label} {field.Maximum} حرفاً.");
            }
        }

        if (command.File is { } file &&
            (string.IsNullOrWhiteSpace(file.FileName) || string.IsNullOrWhiteSpace(file.ContentType) || file.Content.IsEmpty))
        {
            return new AppError(AppErrorKind.Validation, "ملف الوثيقة المحدد غير صالح.");
        }

        return null;
    }
}

public sealed class DeleteTeacherDocumentUseCase(ITeacherDocumentRepository repository)
{
    public Task<Result> ExecuteAsync(int documentId, CancellationToken cancellationToken = default)
    {
        if (documentId <= 0)
        {
            return Task.FromResult(Result.Failure(new AppError(
                AppErrorKind.Validation,
                "معرّف الوثيقة غير صالح.")));
        }

        return repository.DeleteAsync(documentId, cancellationToken);
    }
}
