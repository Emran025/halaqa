using Halaqa.Desktop.Features.TeacherDocuments.Data.Mappers;
using Halaqa.Desktop.Features.TeacherDocuments.Data.Models;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.TeacherDocuments.Data;

public sealed class TeacherDocumentMapperTests
{
    [Fact]
    public void ToUploadDto_TrimsOptionalFieldsAndKeepsInMemoryFile()
    {
        var file = new TeacherDocumentFile("certificate.pdf", "application/pdf", new byte[] { 1, 2, 3 });
        var command = new CreateTeacherDocumentCommand(
            "  إجازة حفص  ",
            "  إجازة  ",
            "   ",
            "  حفص  ",
            "  الرياض  ",
            new DateOnly(2020, 1, 1),
            file);

        var dto = TeacherDocumentMapper.ToUploadDto(command);

        Assert.Equal("إجازة حفص", dto.Name);
        Assert.Equal("إجازة", dto.CertificateType);
        Assert.Null(dto.CertificateTypeOther);
        Assert.Equal("حفص", dto.Riwayah);
        Assert.Equal("certificate.pdf", dto.FileName);
        Assert.Equal(new byte[] { 1, 2, 3 }, dto.FileContent.ToArray());
    }

    [Fact]
    public void ToDomain_MapsPaginatedDocumentCollection()
    {
        var response = new TeacherDocumentCollectionResponseDto(
        [
            new TeacherDocumentDto(
                7,
                "إجازة حفص",
                "إجازة",
                null,
                "حفص",
                "الرياض",
                new DateOnly(2021, 5, 1),
                "https://example.test/7",
                true)
        ],
        new PaginationMetaDto(1, 2, 20, 21));

        var result = TeacherDocumentMapper.ToDomain(response);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(21, result.Value!.Total);
        var document = Assert.Single(result.Value.Documents);
        Assert.Equal("إجازة حفص", document.Name);
        Assert.True(document.HasFile);
    }

    [Fact]
    public void ToDomain_RejectsDocumentWithInvalidIdentifier()
    {
        var result = TeacherDocumentMapper.ToDomain(new TeacherDocumentDto(
            0,
            "إجازة",
            "شهادة",
            null,
            null,
            null,
            null,
            null,
            false));

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }
}
