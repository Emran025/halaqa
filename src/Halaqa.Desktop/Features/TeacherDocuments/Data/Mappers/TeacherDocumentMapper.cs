using Halaqa.Desktop.Features.TeacherDocuments.Data.Models;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.TeacherDocuments.Data.Mappers;

internal static class TeacherDocumentMapper
{
    public static Result<TeacherDocument> ToDomain(TeacherDocumentDto dto)
    {
        if (dto.Id <= 0 ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.CertificateType))
        {
            return Result<TeacherDocument>.Failure(UnexpectedResponseError());
        }

        return Result<TeacherDocument>.Success(new TeacherDocument(
            dto.Id,
            dto.Name,
            dto.CertificateType,
            dto.CertificateTypeOther,
            dto.Riwayah,
            dto.IssuingPlace,
            dto.IssuingDate,
            dto.FileUrl,
            dto.HasFile));
    }

    public static Result<TeacherDocumentPage> ToDomain(TeacherDocumentCollectionResponseDto dto)
    {
        if (dto.Documents is null || dto.Meta is null ||
            dto.Meta.CurrentPage < 1 || dto.Meta.LastPage < 1 ||
            dto.Meta.PerPage < 1 || dto.Meta.Total < 0)
        {
            return Result<TeacherDocumentPage>.Failure(UnexpectedResponseError());
        }

        var documents = dto.Documents.Select(ToDomain).ToArray();
        var error = documents.Select(result => result.Error).FirstOrDefault(item => item is not null);
        if (error is not null)
        {
            return Result<TeacherDocumentPage>.Failure(error);
        }

        return Result<TeacherDocumentPage>.Success(new TeacherDocumentPage(
            documents.Select(result => result.Value!).ToArray(),
            dto.Meta.CurrentPage,
            dto.Meta.LastPage,
            dto.Meta.PerPage,
            dto.Meta.Total));
    }

    public static TeacherDocumentUploadDto ToUploadDto(CreateTeacherDocumentCommand command) => new(
        command.Name.Trim(),
        command.CertificateType.Trim(),
        NormalizeOptional(command.CertificateTypeOther),
        NormalizeOptional(command.Riwayah),
        NormalizeOptional(command.IssuingPlace),
        command.IssuingDate,
        command.File?.FileName,
        command.File?.ContentType,
        command.File?.Content ?? ReadOnlyMemory<byte>.Empty);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AppError UnexpectedResponseError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم بيانات وثيقة معلم بصورة غير متوقعة.");
}
