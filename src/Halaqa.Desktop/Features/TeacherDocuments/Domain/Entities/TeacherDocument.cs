namespace Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;

public sealed record TeacherDocument(
    int Id,
    string Name,
    string CertificateType,
    string? CertificateTypeOther,
    string? Riwayah,
    string? IssuingPlace,
    DateOnly? IssuingDate,
    string? FileUrl,
    bool HasFile);

public sealed record TeacherDocumentPage(
    IReadOnlyList<TeacherDocument> Documents,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record TeacherDocumentFile(
    string FileName,
    string ContentType,
    ReadOnlyMemory<byte> Content);

public sealed record CreateTeacherDocumentCommand(
    string Name,
    string CertificateType,
    string? CertificateTypeOther,
    string? Riwayah,
    string? IssuingPlace,
    DateOnly? IssuingDate,
    TeacherDocumentFile? File);
