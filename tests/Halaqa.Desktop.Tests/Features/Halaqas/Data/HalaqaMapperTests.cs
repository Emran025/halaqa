using System.Text.Json;
using Halaqa.Desktop.Features.Halaqas.Data.Mappers;
using Halaqa.Desktop.Features.Halaqas.Data.Models;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Halaqas.Data;

public sealed class HalaqaMapperTests
{
    [Fact]
    public void ToDto_SerializesCreateCommandUsingContractFieldNames()
    {
        var command = new CreateHalaqaCommand(
            "  حلقة الفجر  ",
            "  مراجعة وحفظ  ",
            HalaqaGender.Male,
            "  السعودية  ",
            "  الرياض  ",
            20,
            "  Asia/Riyadh  ",
            HalaqaStatus.Active);

        var json = JsonSerializer.Serialize(
            HalaqaMapper.ToDto(command),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);

        Assert.Equal("حلقة الفجر", document.RootElement.GetProperty("name").GetString());
        Assert.Equal("male", document.RootElement.GetProperty("gender").GetString());
        Assert.Equal(20, document.RootElement.GetProperty("max_students").GetInt32());
        Assert.Equal("Asia/Riyadh", document.RootElement.GetProperty("timezone").GetString());
        Assert.Equal("active", document.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public void ToDomain_MapsValidHalaqaWithTeacherSummary()
    {
        var result = HalaqaMapper.ToDomain(CreateValidDto());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("حلقة الفجر", result.Value!.Name);
        Assert.Equal(HalaqaStatus.Active, result.Value.Status);
        Assert.Equal("T-100", result.Value.Teacher.TeacherCode);
        Assert.Equal(5, result.Value.AvailableCapacity);
    }

    [Fact]
    public void ToDomain_RejectsUnexpectedStatus()
    {
        var result = HalaqaMapper.ToDomain(CreateValidDto(status: "archived"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }

    private static HalaqaDto CreateValidDto(string status = "active") => new(
        Guid.NewGuid(),
        new HalaqaTeacherDto(
            Guid.NewGuid(),
            "معلم اختبار",
            "T-100",
            "male",
            "السعودية",
            "الرياض",
            "بكالوريوس",
            10,
            true),
        "حلقة الفجر",
        "مراجعة وحفظ",
        status,
        15,
        20,
        5,
        "male",
        "السعودية",
        "الرياض",
        "Asia/Riyadh",
        DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
        DateTimeOffset.Parse("2026-01-02T00:00:00Z"));
}
