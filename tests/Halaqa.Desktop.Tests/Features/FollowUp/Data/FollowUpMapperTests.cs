using System.Text.Json;
using Halaqa.Desktop.Features.FollowUp.Data.Mappers;
using Halaqa.Desktop.Features.FollowUp.Data.Models;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.FollowUp.Data;

public sealed class FollowUpMapperTests
{
    [Fact]
    public void ToDto_SerializesPlanUsingCurrentContractFields()
    {
        var command = new UpdateFollowUpPlanCommand(
            Guid.NewGuid(),
            FollowUpFrequency.TwiceAWeek,
            [new PlanDetailDraft(FollowUpTaskType.Review, FollowUpUnit.HalfHizb, 1.5m, "  مراجعة ثابتة  ")],
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31));

        var json = JsonSerializer.Serialize(FollowUpMapper.ToDto(command), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);

        Assert.Equal("twiceAWeek", document.RootElement.GetProperty("frequency").GetString());
        var detail = document.RootElement.GetProperty("details")[0];
        Assert.Equal("review", detail.GetProperty("task_type").GetString());
        Assert.Equal("halfHizb", detail.GetProperty("unit").GetString());
        Assert.Equal(1.5m, detail.GetProperty("amount").GetDecimal());
        Assert.Equal("مراجعة ثابتة", detail.GetProperty("notes").GetString());
        Assert.Equal("2026-08-01", document.RootElement.GetProperty("starts_on").GetString());
    }

    [Fact]
    public void ToDomain_MapsValidPlanAndAvailability()
    {
        var result = FollowUpMapper.ToDomain(CreateValidPlanResponse());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(FollowUpFrequency.Daily, result.Value!.Frequency);
        Assert.Equal(FollowUpTaskType.Memorization, result.Value.Details[0].TaskType);
        Assert.Equal(new TimeOnly(18, 0), result.Value.AttendancePreferences.WeeklySlots[0].From);
        Assert.Equal(new DateOnly(2026, 8, 1), result.Value.StartsOn);
    }

    [Fact]
    public void ToDomain_RejectsUnexpectedItemState()
    {
        var result = FollowUpMapper.ToDomain(CreateValidItemResponse(state: "unknown"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }

    private static FollowUpPlanResponseDto CreateValidPlanResponse() => new(
        new FollowUpPlanDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "daily",
            "active",
            "Asia/Riyadh",
            [CreateDetail()],
            new AttendancePreferencesDto("Asia/Riyadh", [new WeeklyAvailabilitySlotDto(0, "18:00", "18:30", true)], 30),
            "2026-08-01",
            "2026-08-31",
            1,
            null,
            null,
            DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-08-01T00:00:00Z")));

    private static FollowUpItemResponseDto CreateValidItemResponse(string state) => new(
        new FollowUpItemDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "memorization",
            CreateDetail(),
            DateTimeOffset.Parse("2026-08-01T18:00:00+03:00"),
            "Asia/Riyadh",
            state,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-08-01T00:00:00Z")));

    private static PlanDetailDto CreateDetail() => new(
        Guid.NewGuid(),
        "memorization",
        "page",
        2m,
        "حفظ",
        1,
        DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
        DateTimeOffset.Parse("2026-08-01T00:00:00Z"));
}
