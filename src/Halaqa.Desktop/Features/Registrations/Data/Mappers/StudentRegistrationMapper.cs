using System.Globalization;
using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Data.Mappers;

internal static class StudentRegistrationMapper
{
    public static Result<AvailableTeacher> ToDomain(AvailableTeacherDto dto)
    {
        if (dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.DisplayName) ||
            string.IsNullOrWhiteSpace(dto.TeacherCode))
        {
            return Result<AvailableTeacher>.Failure(UnexpectedResponseError());
        }

        TryParseGender(dto.Gender, out var gender);

        var halaqas = (dto.PublicHalaqas ?? Array.Empty<PublicHalaqaDto>()).Select(ToDomain).ToArray();
        var error = halaqas.Select(result => result.Error).FirstOrDefault(value => value is not null);
        if (error is not null)
        {
            return Result<AvailableTeacher>.Failure(error);
        }

        return Result<AvailableTeacher>.Success(new AvailableTeacher(
            dto.Id,
            dto.DisplayName,
            dto.TeacherCode,
            dto.Avatar,
            gender,
            dto.Country ?? string.Empty,
            dto.City ?? string.Empty,
            dto.Qualification ?? string.Empty,
            dto.ExperienceYears,
            dto.CapacityAvailable,
            dto.Bio,
            dto.ActiveHalaqaCount,
            halaqas.Select(result => result.Value!).ToArray()));
    }

    public static Result<AvailableTeacherPage> ToDomain(TeacherPublicCollectionResponseDto dto)
    {
        if (dto.Teachers is null || dto.Meta is null || dto.Meta.CurrentPage < 1 ||
            dto.Meta.LastPage < 1 || dto.Meta.PerPage < 1 || dto.Meta.Total < 0)
        {
            return Result<AvailableTeacherPage>.Failure(UnexpectedResponseError());
        }

        var teachers = dto.Teachers.Select(ToDomain).ToArray();
        var error = teachers.Select(result => result.Error).FirstOrDefault(value => value is not null);
        if (error is not null)
        {
            return Result<AvailableTeacherPage>.Failure(error);
        }

        return Result<AvailableTeacherPage>.Success(new AvailableTeacherPage(
            teachers.Select(result => result.Value!).ToArray(),
            dto.Meta.CurrentPage,
            dto.Meta.LastPage,
            dto.Meta.PerPage,
            dto.Meta.Total));
    }

    public static CreateStudentRegistrationRequestDto ToDto(CreateStudentRegistrationRequestCommand command) => new(
        NormalizeOptional(command.TeacherCode),
        command.RequestedHalaqaId,
        NormalizeOptional(command.Message),
        ToDto(command.Profile),
        command.PreviousMemorization is { } previous ? ToDto(previous) : null,
        ToDto(command.AttendancePreferences),
        ToDto(command.FollowUpPlan),
        command.ClientOperationId);

    private static Result<PublicHalaqa> ToDomain(PublicHalaqaDto dto)
    {
        if (dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Status) || string.IsNullOrWhiteSpace(dto.Country) ||
            string.IsNullOrWhiteSpace(dto.Residence) || dto.AvailableCapacity is < 0 ||
            !TryParseGender(dto.Gender, out var gender))
        {
            return Result<PublicHalaqa>.Failure(UnexpectedResponseError());
        }

        return Result<PublicHalaqa>.Success(new PublicHalaqa(
            dto.Id,
            dto.Name,
            dto.Status,
            gender,
            dto.Country,
            dto.Residence,
            dto.AvailableCapacity));
    }

    private static StudentApplicationProfileRequestDto ToDto(RegistrationApplicationProfile value) => new(
        ToContractValue(value.Gender),
        value.BirthDate,
        value.Country.Trim(),
        value.City.Trim(),
        NormalizeOptional(value.Residence),
        value.Phone.Trim(),
        value.PhoneZone.Trim(),
        NormalizeOptional(value.WhatsappPhone),
        NormalizeOptional(value.WhatsappZone),
        NormalizeOptional(value.MemorizationLevel),
        NormalizeOptional(value.ReviewLevel),
        NormalizeOptional(value.Bio));

    private static PreviousMemorizationRequestDto ToDto(RegistrationPreviousMemorization value) => new(
        NormalizeOptional(value.MemorizationLevel),
        NormalizeOptional(value.ReviewLevel),
        value.MemorizedJuzCount,
        value.MemorizedSurahIds.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToArray(),
        NormalizeOptional(value.PreviousTeacherNotes),
        NormalizeOptional(value.StopReasons));

    private static RegistrationAttendancePreferencesRequestDto ToDto(RegistrationAttendancePreferences value) => new(
        value.Timezone.Trim(),
        value.WeeklySlots.Select(slot => new RegistrationWeeklyAvailabilitySlotDto(
            slot.DayOfWeek,
            slot.From.ToString("HH:mm", CultureInfo.InvariantCulture),
            slot.To.ToString("HH:mm", CultureInfo.InvariantCulture),
            slot.Preferred)).ToArray(),
        value.PreferredSessionDurationMinutes);

    private static RegistrationFollowUpPlanRequestDto ToDto(RegistrationFollowUpPlan value) => new(
        value.Frequency,
        value.Details.Select(detail => new RegistrationPlanDetailRequestDto(
            detail.TaskType,
            detail.Unit,
            detail.Amount,
            NormalizeOptional(detail.Notes))).ToArray(),
        value.StartsOn,
        value.EndsOn);

    private static bool TryParseGender(string? value, out RegistrationGender gender)
    {
        gender = value switch
        {
            "male" => RegistrationGender.Male,
            "female" => RegistrationGender.Female,
            _ => default
        };
        return value is "male" or "female";
    }

    private static string ToContractValue(RegistrationGender value) => value == RegistrationGender.Male ? "male" : "female";

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AppError UnexpectedResponseError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم بيانات المعلمين العامة بصورة غير متوقعة.");
}
