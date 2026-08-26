using System.Text.Json;
using Halaqa.Desktop.Features.Profile.Data.Mappers;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Profile.Data;

public sealed class TeacherProfileMapperTests
{
    [Fact]
    public void ToDto_SerializesSpecifiedFieldsAndRetainsExplicitNulls()
    {
        var command = new UpdateTeacherProfileCommand(
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<DateOnly?>.Set(null),
            TeacherProfileUpdateField<TeacherGender?>.Set(TeacherGender.Female),
            TeacherProfileUpdateField<string>.Set("السعودية"),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<string>.Set(null),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<int?>.Set(12),
            TeacherProfileUpdateField<string>.Omit(),
            TeacherProfileUpdateField<string>.Set("معلم قرآن"),
            TeacherProfileUpdateField<int?>.Omit());

        var json = JsonSerializer.Serialize(
            TeacherProfileMapper.ToDto(command),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);

        Assert.False(document.RootElement.TryGetProperty("name", out _));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("birth_date").ValueKind);
        Assert.Equal("female", document.RootElement.GetProperty("gender").GetString());
        Assert.Equal("السعودية", document.RootElement.GetProperty("country").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("phone").ValueKind);
        Assert.Equal(12, document.RootElement.GetProperty("experience_years").GetInt32());
        Assert.Equal("معلم قرآن", document.RootElement.GetProperty("bio").GetString());
        Assert.False(document.RootElement.TryGetProperty("max_halaqas", out _));
    }

    [Fact]
    public void ToDomain_MapsContractSupportedDocumentDetails()
    {
        var dto = CreateValidDto(documents:
        new[]
        {
            new TeacherDocumentSummaryDto(
                4,
                "إجازة حفص",
                "إجازة",
                null,
                "حفص",
                "الرياض",
                new DateOnly(2020, 1, 1),
                "https://example.test/document/4",
                true)
        });

        var result = TeacherProfileMapper.ToDomain(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var document = Assert.Single(result.Value!.Documents);
        Assert.Equal("إجازة حفص", document.Name);
        Assert.True(document.HasFile);
        Assert.Equal(TeacherGender.Male, result.Value.Gender);
    }

    [Fact]
    public void ToDomain_ReturnsUnknownErrorForUnexpectedGender()
    {
        var result = TeacherProfileMapper.ToDomain(CreateValidDto(gender: "unknown"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }

    private static TeacherProfileDto CreateValidDto(
        string gender = "male",
        IReadOnlyList<TeacherDocumentSummaryDto>? documents = null) => new(
        Guid.NewGuid(),
        "معلم اختبار",
        "T-1001",
        null,
        gender,
        "السعودية",
        "الرياض",
        "بكالوريوس",
        8,
        true,
        "نبذة تعريفية",
        1,
        Array.Empty<TeacherHalaqaSummaryDto>(),
        new DateOnly(1990, 1, 1),
        "teacher@example.test",
        null,
        null,
        null,
        null,
        null,
        "المساء",
        documents ?? Array.Empty<TeacherDocumentSummaryDto>());
}
