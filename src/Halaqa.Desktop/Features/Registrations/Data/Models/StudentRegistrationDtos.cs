using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Registrations.Data.Models;

internal sealed record PublicHalaqaDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("residence")] string Residence,
    [property: JsonPropertyName("available_capacity")] int? AvailableCapacity);

internal sealed record AvailableTeacherDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("teacher_code")] string TeacherCode,
    [property: JsonPropertyName("avatar")] string? Avatar,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("qualification")] string Qualification,
    [property: JsonPropertyName("experience_years")] int ExperienceYears,
    [property: JsonPropertyName("capacity_available")] bool CapacityAvailable,
    [property: JsonPropertyName("bio")] string? Bio,
    [property: JsonPropertyName("active_halaqa_count")] int? ActiveHalaqaCount,
    [property: JsonPropertyName("public_halaqas")] IReadOnlyList<PublicHalaqaDto>? PublicHalaqas);

internal sealed record TeacherPublicResponseDto(
    [property: JsonPropertyName("teacher")] AvailableTeacherDto Teacher);

internal sealed record TeacherPublicCollectionResponseDto(
    [property: JsonPropertyName("teachers")] IReadOnlyList<AvailableTeacherDto> Teachers,
    [property: JsonPropertyName("meta")] RegistrationPaginationMetaDto Meta);

internal sealed record StudentApplicationProfileRequestDto(
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("birth_date")] DateOnly BirthDate,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("residence")] string? Residence,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("phone_zone")] string PhoneZone,
    [property: JsonPropertyName("whatsapp_phone")] string? WhatsappPhone,
    [property: JsonPropertyName("whatsapp_zone")] string? WhatsappZone,
    [property: JsonPropertyName("memorization_level")] string? MemorizationLevel,
    [property: JsonPropertyName("review_level")] string? ReviewLevel,
    [property: JsonPropertyName("bio")] string? Bio);

internal sealed record PreviousMemorizationRequestDto(
    [property: JsonPropertyName("memorization_level")] string? MemorizationLevel,
    [property: JsonPropertyName("review_level")] string? ReviewLevel,
    [property: JsonPropertyName("memorized_juz_count")] decimal? MemorizedJuzCount,
    [property: JsonPropertyName("memorized_surah_ids")] IReadOnlyList<string> MemorizedSurahIds,
    [property: JsonPropertyName("previous_teacher_notes")] string? PreviousTeacherNotes,
    [property: JsonPropertyName("stop_reasons")] string? StopReasons);

internal sealed record RegistrationWeeklyAvailabilitySlotDto(
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("preferred")] bool Preferred);

internal sealed record RegistrationAttendancePreferencesRequestDto(
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("weekly_slots")] IReadOnlyList<RegistrationWeeklyAvailabilitySlotDto> WeeklySlots,
    [property: JsonPropertyName("preferred_session_duration_minutes")] int? PreferredSessionDurationMinutes);

internal sealed record RegistrationPlanDetailRequestDto(
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("notes")] string? Notes);

internal sealed record RegistrationFollowUpPlanRequestDto(
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("details")] IReadOnlyList<RegistrationPlanDetailRequestDto> Details,
    [property: JsonPropertyName("starts_on")] DateOnly? StartsOn,
    [property: JsonPropertyName("ends_on")] DateOnly? EndsOn);

internal sealed record CreateStudentRegistrationRequestDto(
    [property: JsonPropertyName("teacher_code")] string? TeacherCode,
    [property: JsonPropertyName("requested_halaqa_id")] Guid? RequestedHalaqaId,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("profile")] StudentApplicationProfileRequestDto Profile,
    [property: JsonPropertyName("previous_memorization")] PreviousMemorizationRequestDto? PreviousMemorization,
    [property: JsonPropertyName("attendance_preferences")] RegistrationAttendancePreferencesRequestDto AttendancePreferences,
    [property: JsonPropertyName("follow_up_plan")] RegistrationFollowUpPlanRequestDto FollowUpPlan,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);
