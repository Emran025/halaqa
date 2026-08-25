using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.TeacherDocuments.Data.Models;

internal sealed record TeacherDocumentDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("certificate_type")] string CertificateType,
    [property: JsonPropertyName("certificate_type_other")] string? CertificateTypeOther,
    [property: JsonPropertyName("riwayah")] string? Riwayah,
    [property: JsonPropertyName("issuing_place")] string? IssuingPlace,
    [property: JsonPropertyName("issuing_date")] DateOnly? IssuingDate,
    [property: JsonPropertyName("file_url")] string? FileUrl,
    [property: JsonPropertyName("has_file")] bool HasFile);

internal sealed record TeacherDocumentResponseDto(
    [property: JsonPropertyName("teacher_document")] TeacherDocumentDto TeacherDocument);

internal sealed record TeacherDocumentCollectionResponseDto(
    [property: JsonPropertyName("documents")] IReadOnlyList<TeacherDocumentDto> Documents,
    [property: JsonPropertyName("meta")] PaginationMetaDto Meta);

internal sealed record PaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record TeacherDocumentUploadDto(
    string Name,
    string CertificateType,
    string? CertificateTypeOther,
    string? Riwayah,
    string? IssuingPlace,
    DateOnly? IssuingDate,
    string? FileName,
    string? FileContentType,
    ReadOnlyMemory<byte> FileContent);
