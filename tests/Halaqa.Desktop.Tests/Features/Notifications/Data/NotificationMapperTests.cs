using Halaqa.Desktop.Features.Notifications.Data.Mappers;
using Halaqa.Desktop.Features.Notifications.Data.Models;
using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Notifications.Data;

public sealed class NotificationMapperTests
{
    [Fact]
    public void ToDomain_MapsValidNotificationCollection()
    {
        var result = NotificationMapper.ToDomain(CreateValidResponse());

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(result.Value!.Notifications);
        Assert.Equal(NotificationType.FollowUpDue, notification.Type);
        Assert.Equal(NotificationEntityType.FollowUpItem, notification.Payload.EntityType);
        Assert.Equal(NotificationAction.Reschedule, notification.Payload.Action);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void ToDomain_RejectsUnknownNotificationType()
    {
        var result = NotificationMapper.ToDomain(CreateValidResponse(type: "unknown"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }

    private static NotificationCollectionResponseDto CreateValidResponse(string type = "follow_up_due") => new(
        new[] {new NotificationDto(
            Guid.NewGuid(),
            type,
            "متابعة مستحقة",
            "لديك مراجعة مستحقة اليوم.",
            new NotificationPayloadDto(
                "follow_up_due",
                "follow_up_item",
                Guid.NewGuid(),
                null,
                Guid.NewGuid(),
                "reschedule",
                "/follow-up-items/123"),
            null,
            DateTimeOffset.Parse("2026-08-26T09:00:00Z"))},
        new NotificationPaginationMetaDto(1, 1, 20, 1));
}
