using System.Text.Json;
using Halaqa.Desktop.Features.Memberships.Data.Mappers;
using Halaqa.Desktop.Features.Memberships.Data.Models;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Memberships.Data;

public sealed class HalaqaMembershipMapperTests
{
    [Fact]
    public void ToDto_SerializesAssignmentAndStatusUpdateUsingContractNames()
    {
        var halaqaId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var assignmentJson = JsonSerializer.Serialize(
            HalaqaMembershipMapper.ToDto(new AssignStudentToHalaqaCommand(halaqaId, studentId)),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var updateJson = JsonSerializer.Serialize(
            HalaqaMembershipMapper.ToDto(new UpdateHalaqaMembershipCommand(
                halaqaId,
                membershipId,
                MembershipStatus.Inactive,
                "  مؤقتاً  ")),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        using var assignment = JsonDocument.Parse(assignmentJson);
        using var update = JsonDocument.Parse(updateJson);
        Assert.Equal(studentId, assignment.RootElement.GetProperty("student_id").GetGuid());
        Assert.Equal("inactive", update.RootElement.GetProperty("status").GetString());
        Assert.Equal("مؤقتاً", update.RootElement.GetProperty("reason").GetString());
    }

    [Fact]
    public void ToDomain_MapsMembershipCollectionWithMembershipIdentifiers()
    {
        var response = new MembershipCollectionResponseDto(
        new[] {
            CreateValidDto()
        },
        new MembershipPaginationMetaDto(1, 1, 20, 1));

        var result = HalaqaMembershipMapper.ToDomain(response);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var membership = Assert.Single(result.Value!.Memberships);
        Assert.NotEqual(Guid.Empty, membership.Id);
        Assert.Equal("طالب اختبار", membership.Student.Name);
        Assert.Equal(MembershipStatus.Active, membership.Status);
    }

    [Fact]
    public void ToDomain_RejectsMembershipWithNonStudentUser()
    {
        var dto = CreateValidDto(studentRole: "teacher");

        var result = HalaqaMembershipMapper.ToDomain(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }

    private static HalaqaMembershipDto CreateValidDto(string studentRole = "student") => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        new MembershipStudentDto(
            Guid.NewGuid(),
            studentRole,
            "طالب اختبار",
            "student@example.test",
            null,
            "active",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-01-02T00:00:00Z")
        ),
        "active",
        DateTimeOffset.Parse("2026-02-01T00:00:00Z"));
}
