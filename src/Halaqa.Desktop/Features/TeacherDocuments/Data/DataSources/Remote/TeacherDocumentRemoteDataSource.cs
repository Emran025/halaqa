using System.Net.Http.Headers;
using System.Text;
using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.TeacherDocuments.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.TeacherDocuments.Data.DataSources.Remote;

internal interface ITeacherDocumentRemoteDataSource
{
    Task<Result<TeacherDocumentCollectionResponseDto>> ListAsync(
        int page,
        CancellationToken cancellationToken = default);

    Task<Result<TeacherDocumentResponseDto>> CreateAsync(
        TeacherDocumentUploadDto upload,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default);
}

internal sealed class TeacherDocumentRemoteDataSource(IApiClient apiClient) : ITeacherDocumentRemoteDataSource
{
    public Task<Result<TeacherDocumentCollectionResponseDto>> ListAsync(
        int page,
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<TeacherDocumentCollectionResponseDto>(
            $"me/teacher-documents?page={page}",
            cancellationToken);

    public async Task<Result<TeacherDocumentResponseDto>> CreateAsync(
        TeacherDocumentUploadDto upload,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        AddString(content, "name", upload.Name);
        AddString(content, "certificate_type", upload.CertificateType);
        AddOptionalString(content, "certificate_type_other", upload.CertificateTypeOther);
        AddOptionalString(content, "riwayah", upload.Riwayah);
        AddOptionalString(content, "issuing_place", upload.IssuingPlace);
        if (upload.IssuingDate is { } issuingDate)
        {
            AddString(content, "issuing_date", issuingDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
        }
        if (!upload.FileContent.IsEmpty && !string.IsNullOrWhiteSpace(upload.FileName))
        {
            var fileContent = new ByteArrayContent(upload.FileContent.ToArray());
            if (!string.IsNullOrWhiteSpace(upload.FileContentType))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(upload.FileContentType);
            }
            content.Add(fileContent, "file", upload.FileName);
        }

        return await apiClient.PostMultipartAsync<TeacherDocumentResponseDto>(
            "me/teacher-documents",
            content,
            cancellationToken);
    }

    public Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default) =>
        apiClient.DeleteAsync($"me/teacher-documents/{documentId}", cancellationToken);

    private static void AddString(MultipartFormDataContent content, string name, string value) =>
        content.Add(new StringContent(value, Encoding.UTF8), name);

    private static void AddOptionalString(MultipartFormDataContent content, string name, string? value)
    {
        if (value is not null)
        {
            AddString(content, name, value);
        }
    }
}
