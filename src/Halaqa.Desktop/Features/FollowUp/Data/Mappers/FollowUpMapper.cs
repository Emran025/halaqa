using System.Globalization;
using Halaqa.Desktop.Features.FollowUp.Data.Models;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Data.Mappers;

internal static class FollowUpMapper
{
    public static Result<FollowUpPlan> ToDomain(FollowUpPlanResponseDto dto) => ToDomain(dto.FollowUpPlan);

    public static Result<FollowUpPlan> ToDomain(FollowUpPlanDto dto)
    {
        if (dto is null || dto.Id == Guid.Empty || dto.StudentId == Guid.Empty || dto.CreatedByUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.Status) || string.IsNullOrWhiteSpace(dto.Timezone) || dto.Version < 1 ||
            dto.Details is null ||
            !TryParseEnum(dto.Frequency, out FollowUpFrequency frequency) ||
            !TryParseDate(dto.StartsOn, out var startsOn) || !TryParseDate(dto.EndsOn, out var endsOn))
        {
            return Result<FollowUpPlan>.Failure(UnexpectedResponseError());
        }

        var details = dto.Details.Select(ToDomain).ToArray();
        var detailError = details.Select(item => item.Error).FirstOrDefault(error => error is not null);
        if (detailError is not null)
        {
            return Result<FollowUpPlan>.Failure(detailError);
        }

        var attendance = dto.AttendancePreferences is not null
            ? ToDomain(dto.AttendancePreferences).Value ?? new AttendancePreferences(dto.Timezone, Array.Empty<WeeklyAvailabilitySlot>(), null)
            : new AttendancePreferences(dto.Timezone, Array.Empty<WeeklyAvailabilitySlot>(), null);

        return Result<FollowUpPlan>.Success(new FollowUpPlan(
            dto.Id,
            dto.StudentId,
            dto.CreatedByUserId,
            dto.SourceRegistrationRequestId,
            frequency,
            dto.Status,
            dto.Timezone,
            details.Select(item => item.Value!).ToArray(),
            attendance,
            startsOn,
            endsOn,
            dto.Version,
            dto.ApprovedByUserId,
            dto.ApprovedAt,
            dto.CreatedAt,
            dto.UpdatedAt));
    }

    public static Result<AttendancePreferences> ToDomain(AttendancePreferencesResponseDto dto) => ToDomain(dto.AttendancePreferences);

    public static Result<AttendancePreferences> ToDomain(AttendancePreferencesDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Timezone) || dto.WeeklySlots is null ||
            dto.PreferredSessionDurationMinutes is < 10 or > 180)
        {
            return Result<AttendancePreferences>.Failure(UnexpectedResponseError());
        }

        var slots = dto.WeeklySlots.Select(ToDomain).ToArray();
        var slotError = slots.Select(item => item.Error).FirstOrDefault(error => error is not null);
        if (slotError is not null)
        {
            return Result<AttendancePreferences>.Failure(slotError);
        }

        return Result<AttendancePreferences>.Success(new AttendancePreferences(
            dto.Timezone,
            slots.Select(item => item.Value!).ToArray(),
            dto.PreferredSessionDurationMinutes));
    }

    public static Result<FollowUpItem> ToDomain(FollowUpItemResponseDto dto) => ToDomain(dto.FollowUpItem);

    public static Result<FollowUpItem> ToDomain(FollowUpItemDto dto)
    {
        if (dto is null || dto.Id == Guid.Empty || dto.PlanId == Guid.Empty || dto.PlanDetailId == Guid.Empty ||
            dto.StudentId == Guid.Empty || dto.PlanDetail is null || string.IsNullOrWhiteSpace(dto.Timezone) ||
            !TryParseEnum(dto.TaskType, out FollowUpTaskType taskType) ||
            !TryParseEnum(dto.State, out FollowUpItemState state))
        {
            return Result<FollowUpItem>.Failure(UnexpectedResponseError());
        }

        var detail = ToDomain(dto.PlanDetail);
        if (!detail.IsSuccess || detail.Value is null)
        {
            return Result<FollowUpItem>.Failure(detail.Error!);
        }

        return Result<FollowUpItem>.Success(new FollowUpItem(
            dto.Id,
            dto.PlanId,
            dto.PlanDetailId,
            dto.StudentId,
            dto.HalaqaId,
            taskType,
            detail.Value,
            dto.ScheduledFor,
            dto.Timezone,
            state,
            dto.CompletedAt,
            dto.SkippedAt,
            dto.SkipReason,
            dto.RescheduledFromId,
            dto.NotificationSentAt,
            dto.CreatedAt,
            dto.UpdatedAt));
    }

    public static Result<FollowUpItemPage> ToDomain(FollowUpItemCollectionResponseDto dto)
    {
        if (dto is null || dto.FollowUpItems is null || !IsValidMeta(dto.Meta))
        {
            return Result<FollowUpItemPage>.Failure(UnexpectedResponseError());
        }

        var items = dto.FollowUpItems.Select(ToDomain).ToArray();
        var itemError = items.Select(item => item.Error).FirstOrDefault(error => error is not null);
        return itemError is not null
            ? Result<FollowUpItemPage>.Failure(itemError)
            : Result<FollowUpItemPage>.Success(new FollowUpItemPage(
                items.Select(item => item.Value!).ToArray(), dto.Meta.CurrentPage, dto.Meta.LastPage, dto.Meta.PerPage, dto.Meta.Total));
    }

    public static Result<TrackingPage> ToDomain(TrackingCollectionResponseDto dto)
    {
        if (dto is null || dto.Trackings is null || !IsValidMeta(dto.Meta))
        {
            return Result<TrackingPage>.Failure(UnexpectedResponseError());
        }

        var items = dto.Trackings.Select(ToDomain).ToArray();
        var itemError = items.Select(item => item.Error).FirstOrDefault(error => error is not null);
        return itemError is not null
            ? Result<TrackingPage>.Failure(itemError)
            : Result<TrackingPage>.Success(new TrackingPage(
                items.Select(item => item.Value!).ToArray(), dto.Meta.CurrentPage, dto.Meta.LastPage, dto.Meta.PerPage, dto.Meta.Total));
    }

    public static FollowUpPlanInputDto ToDto(UpdateFollowUpPlanCommand command) => new(
        ToContractValue(command.Frequency),
        command.Details.Select(detail => new PlanDetailInputDto(
            ToContractValue(detail.TaskType),
            ToContractValue(detail.Unit),
            detail.Amount,
            NormalizeOptional(detail.Notes))).ToArray(),
        ToDateString(command.StartsOn),
        ToDateString(command.EndsOn));

    public static AttendancePreferencesDto ToDto(AttendancePreferences preferences) => new(
        preferences.Timezone.Trim(),
        preferences.WeeklySlots.Select(slot => new WeeklyAvailabilitySlotDto(
            slot.DayOfWeek,
            slot.From.ToString("HH:mm", CultureInfo.InvariantCulture),
            slot.To.ToString("HH:mm", CultureInfo.InvariantCulture),
            slot.Preferred)).ToArray(),
        preferences.PreferredSessionDurationMinutes);

    private static Result<FollowUpPlanDetail> ToDomain(PlanDetailDto dto)
    {
        if (dto is null || dto.Id == Guid.Empty || dto.Amount <= 0 || dto.SortOrder < 1 ||
            !TryParseEnum(dto.TaskType, out FollowUpTaskType taskType) || !TryParseEnum(dto.Unit, out FollowUpUnit unit))
        {
            return Result<FollowUpPlanDetail>.Failure(UnexpectedResponseError());
        }

        return Result<FollowUpPlanDetail>.Success(new FollowUpPlanDetail(
            dto.Id, taskType, unit, dto.Amount, dto.Notes, dto.SortOrder, dto.CreatedAt, dto.UpdatedAt));
    }

    private static Result<WeeklyAvailabilitySlot> ToDomain(WeeklyAvailabilitySlotDto dto)
    {
        if (dto is null || dto.DayOfWeek is < 0 or > 6 ||
            !TimeOnly.TryParseExact(dto.From, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) ||
            !TimeOnly.TryParseExact(dto.To, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to) || from >= to)
        {
            return Result<WeeklyAvailabilitySlot>.Failure(UnexpectedResponseError());
        }

        return Result<WeeklyAvailabilitySlot>.Success(new WeeklyAvailabilitySlot(dto.DayOfWeek, from, to, dto.Preferred));
    }

    private static Result<TrackingItem> ToDomain(TrackingDto dto)
    {
        if (dto is null || dto.Id == Guid.Empty || dto.StudentId == Guid.Empty ||
            !DateOnly.TryParseExact(dto.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
            !TryParseEnum(dto.AttendanceType, out AttendanceType attendanceType) || dto.BehaviorNote is < 0 or > 100)
        {
            return Result<TrackingItem>.Failure(UnexpectedResponseError());
        }

        return Result<TrackingItem>.Success(new TrackingItem(
            dto.Id, dto.StudentId, dto.HalaqaId, date, attendanceType, dto.Note, dto.BehaviorNote, dto.CreatedAt, dto.UpdatedAt));
    }

    private static bool IsValidMeta(PaginationMetaDto? meta) =>
        meta is not null && meta.CurrentPage >= 1 && meta.LastPage >= 1 && meta.PerPage >= 1 && meta.Total >= 0;

    private static bool TryParseDate(string? value, out DateOnly? date)
    {
        if (value is null)
        {
            date = null;
            return true;
        }

        var parsed = DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
        date = parsed ? result : null;
        return parsed;
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum
    {
        var normalized = string.Concat((value ?? string.Empty).Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
        return Enum.TryParse(normalized, ignoreCase: true, out parsed);
    }

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? ToDateString(DateOnly? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static AppError UnexpectedResponseError() => new(AppErrorKind.Unknown, "أعاد الخادم بيانات متابعة بصورة غير متوقعة.");
}
