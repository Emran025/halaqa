using System.Globalization;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.Mappers;

internal static class StudentProfileMapper
{
    public static Result<StudentProfile> ToDomain(StudentProfileDto dto)
    {
        if (dto.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Status) ||
            !TryParse(dto.Gender, out StudentGender gender) ||
            !TryParse(dto.Visibility, out StudentProfileVisibility visibility) ||
            !string.Equals(dto.Role, "student", StringComparison.OrdinalIgnoreCase))
        {
            return UnexpectedResponse();
        }

        var previousMemorization = ToPreviousMemorization(dto.PreviousMemorization);
        if (!previousMemorization.IsSuccess)
        {
            return Result<StudentProfile>.Failure(previousMemorization.Error!);
        }

        var attendancePreferences = ToAttendancePreferences(dto.AttendancePreferences);
        if (!attendancePreferences.IsSuccess)
        {
            return Result<StudentProfile>.Failure(attendancePreferences.Error!);
        }

        var followUpPlan = ToFollowUpPlan(dto.FollowUpPlan);
        if (!followUpPlan.IsSuccess)
        {
            return Result<StudentProfile>.Failure(followUpPlan.Error!);
        }

        return Result<StudentProfile>.Success(new StudentProfile(
            dto.Id,
            dto.Name,
            dto.Email,
            dto.Status,
            dto.BirthDate,
            gender,
            dto.Country,
            dto.City,
            dto.Residence,
            dto.Phone,
            dto.PhoneZone,
            dto.WhatsappPhone,
            dto.WhatsappZone,
            dto.MemorizationLevel,
            dto.ReviewLevel,
            previousMemorization.Value,
            attendancePreferences.Value,
            followUpPlan.Value,
            visibility));
    }

    public static UpdateStudentProfileRequestDto ToDto(UpdateStudentProfileCommand command) => new(
        command.Name.IsSpecified,
        command.Name.Value,
        command.BirthDate.IsSpecified,
        command.BirthDate.Value,
        command.Gender.IsSpecified,
        command.Gender.Value is { } gender ? ToContractValue(gender) : null,
        command.Country.IsSpecified,
        command.Country.Value,
        command.City.IsSpecified,
        command.City.Value,
        command.Residence.IsSpecified,
        command.Residence.Value,
        command.Phone.IsSpecified,
        command.Phone.Value,
        command.PhoneZone.IsSpecified,
        command.PhoneZone.Value,
        command.WhatsappPhone.IsSpecified,
        command.WhatsappPhone.Value,
        command.WhatsappZone.IsSpecified,
        command.WhatsappZone.Value,
        command.MemorizationLevel.IsSpecified,
        command.MemorizationLevel.Value,
        command.ReviewLevel.IsSpecified,
        command.ReviewLevel.Value,
        command.PreviousMemorization.IsSpecified,
        command.PreviousMemorization.Value is { } previous ? ToDto(previous) : null,
        command.AttendancePreferences.IsSpecified,
        command.AttendancePreferences.Value is { } attendance ? ToDto(attendance) : null,
        command.FollowUpPlan.IsSpecified,
        command.FollowUpPlan.Value is { } followUpPlan ? ToDto(followUpPlan) : null,
        command.Bio.IsSpecified,
        command.Bio.Value);

    private static Result<StudentPreviousMemorization?> ToPreviousMemorization(StudentPreviousMemorizationDto? dto)
    {
        if (dto is null)
        {
            return Result<StudentPreviousMemorization?>.Success(null);
        }

        var lastCompletedUnit = ToPlanDetail(dto.LastCompletedUnit);
        if (!lastCompletedUnit.IsSuccess)
        {
            return Result<StudentPreviousMemorization?>.Failure(lastCompletedUnit.Error!);
        }

        return Result<StudentPreviousMemorization?>.Success(new StudentPreviousMemorization(
            dto.MemorizationLevel,
            dto.ReviewLevel,
            dto.MemorizedJuzCount,
            dto.MemorizedSurahIds ?? Array.Empty<string>(),
            lastCompletedUnit.Value,
            dto.PreviousTeacherNotes,
            dto.StopReasons));
    }

    private static Result<StudentAttendancePreferences?> ToAttendancePreferences(StudentAttendancePreferencesDto? dto)
    {
        if (dto is null)
        {
            return Result<StudentAttendancePreferences?>.Success(null);
        }

        if (string.IsNullOrWhiteSpace(dto.Timezone) || dto.WeeklySlots is null)
        {
            return UnexpectedResponse<StudentAttendancePreferences?>();
        }

        var slots = new List<StudentWeeklyAvailabilitySlot>();
        foreach (var slot in dto.WeeklySlots)
        {
            if (slot.DayOfWeek is < 0 or > 6 ||
                !TimeOnly.TryParseExact(slot.From, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) ||
                !TimeOnly.TryParseExact(slot.To, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to) ||
                from >= to)
            {
                return UnexpectedResponse<StudentAttendancePreferences?>();
            }

            slots.Add(new StudentWeeklyAvailabilitySlot(slot.DayOfWeek, from, to, slot.Preferred));
        }

        return Result<StudentAttendancePreferences?>.Success(new StudentAttendancePreferences(
            dto.Timezone,
            slots,
            dto.PreferredSessionDurationMinutes));
    }

    private static Result<StudentFollowUpPlan?> ToFollowUpPlan(StudentFollowUpPlanDto? dto)
    {
        if (dto is null)
        {
            return Result<StudentFollowUpPlan?>.Success(null);
        }

        if (dto.Id == Guid.Empty || dto.StudentId == Guid.Empty || dto.CreatedByUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.Status) || string.IsNullOrWhiteSpace(dto.Timezone) ||
            dto.Details is null || dto.Details.Count == 0 || !TryParse(dto.Frequency, out FollowUpFrequency frequency))
        {
            return UnexpectedResponse<StudentFollowUpPlan?>();
        }

        var details = dto.Details.Select(ToPlanDetail).ToArray();
        var detailError = details.Select(result => result.Error).FirstOrDefault(error => error is not null);
        if (detailError is not null)
        {
            return Result<StudentFollowUpPlan?>.Failure(detailError);
        }

        var attendance = ToAttendancePreferences(dto.AttendancePreferences);
        if (!attendance.IsSuccess || attendance.Value is null)
        {
            return Result<StudentFollowUpPlan?>.Failure(attendance.Error ?? UnexpectedResponse().Error!);
        }

        return Result<StudentFollowUpPlan?>.Success(new StudentFollowUpPlan(
            dto.Id,
            dto.StudentId,
            dto.CreatedByUserId,
            dto.SourceRegistrationRequestId,
            frequency,
            dto.Status,
            dto.Timezone,
            details.Select(result => result.Value!).ToArray(),
            attendance.Value,
            dto.StartsOn,
            dto.EndsOn,
            dto.Version,
            dto.ApprovedByUserId,
            dto.ApprovedAt,
            dto.CreatedAt,
            dto.UpdatedAt));
    }

    private static Result<StudentPlanDetail?> ToPlanDetail(StudentPlanDetailDto? dto)
    {
        if (dto is null)
        {
            return Result<StudentPlanDetail?>.Success(null);
        }

        if (dto.Id == Guid.Empty || dto.Amount <= 0 || dto.SortOrder < 1 ||
            !TryParse(dto.TaskType, out QuranTaskType taskType) ||
            !TryParse(dto.Unit, out QuranPlanUnit unit))
        {
            return UnexpectedResponse<StudentPlanDetail?>();
        }

        return Result<StudentPlanDetail?>.Success(new StudentPlanDetail(
            dto.Id,
            taskType,
            unit,
            dto.Amount,
            dto.Notes,
            dto.SortOrder,
            dto.CreatedAt,
            dto.UpdatedAt));
    }

    private static StudentPreviousMemorizationRequestDto ToDto(StudentPreviousMemorization value) => new(
        value.MemorizationLevel,
        value.ReviewLevel,
        value.MemorizedJuzCount,
        value.MemorizedSurahIds,
        value.LastCompletedUnit is { } lastCompletedUnit ? ToDto(lastCompletedUnit) : null,
        value.PreviousTeacherNotes,
        value.StopReasons);

    private static StudentAttendancePreferencesRequestDto ToDto(StudentAttendancePreferences value) => new(
        value.Timezone,
        value.WeeklySlots.Select(slot => new StudentWeeklyAvailabilitySlotDto(
            slot.DayOfWeek,
            slot.From.ToString("HH:mm", CultureInfo.InvariantCulture),
            slot.To.ToString("HH:mm", CultureInfo.InvariantCulture),
            slot.Preferred)).ToArray(),
        value.PreferredSessionDurationMinutes);

    private static StudentFollowUpPlanInputDto ToDto(StudentFollowUpPlanDraft value) => new(
        ToContractValue(value.Frequency),
        value.Details.Select(detail => new StudentPlanDetailInputDto(
            ToContractValue(detail.TaskType),
            ToContractValue(detail.Unit),
            detail.Amount,
            detail.Notes)).ToArray(),
        value.StartsOn,
        value.EndsOn);

    private static StudentPlanDetailDto ToDto(StudentPlanDetail value) => new(
        value.Id,
        ToContractValue(value.TaskType),
        ToContractValue(value.Unit),
        value.Amount,
        value.Notes,
        value.SortOrder,
        value.CreatedAt,
        value.UpdatedAt);

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static bool TryParse<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out parsed);

    private static Result<StudentProfile> UnexpectedResponse() =>
        Result<StudentProfile>.Failure(CreateUnexpectedResponseError());

    private static Result<T> UnexpectedResponse<T>() =>
        Result<T>.Failure(CreateUnexpectedResponseError());

    private static AppError CreateUnexpectedResponseError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم بيانات ملف الطالب بصورة غير متوقعة.");
}
