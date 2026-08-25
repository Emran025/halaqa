using System.Text.Json;
using Halaqa.Desktop.Features.Profile.Data.Mappers;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Profile.Data;

public sealed class ProfileMapperTests
{
    [Fact]
    public void ToDto_SerializesOnlySpecifiedFieldsAndKeepsExplicitNull()
    {
        var command = new UpdateUserProfileCommand(
            ProfileUpdateField<string>.Set("طالب اختبار"),
            ProfileUpdateField<string>.Set(null),
            ProfileUpdateField<string>.Omit(),
            ProfileUpdateField<string>.Set("ثلاثة أجزاء"));

        var json = JsonSerializer.Serialize(ProfileMapper.ToDto(command), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);

        Assert.Equal("طالب اختبار", document.RootElement.GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("phone").ValueKind);
        Assert.False(document.RootElement.TryGetProperty("memorization_level", out _));
        Assert.Equal("ثلاثة أجزاء", document.RootElement.GetProperty("review_level").GetString());
    }

    [Fact]
    public void ToDomain_ReturnsUnknownErrorForUnexpectedRole()
    {
        var dto = new UserProfileDto(
            Guid.NewGuid(),
            "administrator",
            "مستخدم اختبار",
            "user@example.test",
            null,
            "active");

        var result = ProfileMapper.ToDomain(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }
}
