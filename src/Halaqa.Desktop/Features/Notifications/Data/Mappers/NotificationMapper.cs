using Halaqa.Desktop.Features.Notifications.Data.Models;
using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Data.Mappers;

internal static class NotificationMapper
{
    public static Result<NotificationPage> ToDomain(NotificationCollectionResponseDto dto)
    {
        if (dto is null || dto.Notifications is null || dto.Meta is null ||
            dto.Meta.CurrentPage < 1 || dto.Meta.LastPage < 1 || dto.Meta.PerPage < 1 || dto.Meta.Total < 0)
        {
            return Result<NotificationPage>.Failure(UnexpectedResponseError());
        }

        var notifications = dto.Notifications.Select(ToDomain).ToArray();
        var error = notifications.Select(item => item.Error).FirstOrDefault(value => value is not null);
        return error is not null
            ? Result<NotificationPage>.Failure(error)
            : Result<NotificationPage>.Success(new NotificationPage(
                notifications.Select(item => item.Value!).ToArray(),
                dto.Meta.CurrentPage,
                dto.Meta.LastPage,
                dto.Meta.PerPage,
                dto.Meta.Total));
    }

    private static Result<HalaqaNotification> ToDomain(NotificationDto dto)
    {
        if (dto is null || dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Body) ||
            !TryParseEnum(dto.Type, out NotificationType type))
        {
            return Result<HalaqaNotification>.Failure(UnexpectedResponseError());
        }

        var payloadResult = ToDomain(dto.Payload ?? new NotificationPayloadDto());
        var payload = payloadResult.Value ?? new NotificationPayload(null, NotificationEntityType.Halaqa, Guid.Empty, null, null, NotificationAction.Open, null);

        return Result<HalaqaNotification>.Success(new HalaqaNotification(
            dto.Id, type, dto.Title, dto.Body, payload, dto.ReadAt, dto.CreatedAt));
    }

    private static Result<NotificationPayload> ToDomain(NotificationPayloadDto dto)
    {
        if (dto is null)
        {
            return Result<NotificationPayload>.Success(new NotificationPayload(null, NotificationEntityType.Halaqa, Guid.Empty, null, null, NotificationAction.Open, null));
        }

        TryParseEnum(dto.EntityType, out NotificationEntityType entityType);
        TryParseEnum(dto.Action, out NotificationAction action);

        return Result<NotificationPayload>.Success(new NotificationPayload(
            dto.EventType,
            entityType,
            dto.EntityId ?? Guid.Empty,
            dto.SessionId,
            dto.FollowUpItemId,
            action,
            dto.ActionPath));
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum
    {
        var normalized = string.Concat((value ?? string.Empty).Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
        return Enum.TryParse(normalized, ignoreCase: true, out parsed);
    }

    private static AppError UnexpectedResponseError() => new(AppErrorKind.Unknown, "أعاد الخادم بيانات إشعار بصورة غير متوقعة.");
}
