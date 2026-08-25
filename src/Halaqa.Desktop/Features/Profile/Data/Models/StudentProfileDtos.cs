using System.Text.Json;
using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Profile.Data.Models;

internal sealed record StudentProfileResponseDto(
    [property: JsonPropertyName("student_profile")] StudentProfileDto StudentProfile);

internal sealed record StudentProfileDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("birth_date")] DateOnly? BirthDate,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string? Country,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("residence")] string? Residence,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("phone_zone")] string? PhoneZone,
    [property: JsonPropertyName("whatsapp_phone")] string? WhatsappPhone,
    [property: JsonPropertyName("whatsapp_zone")] string? WhatsappZone,
    [property: JsonPropertyName("memorization_level")] string? MemorizationLevel,
    [property: JsonPropertyName("review_level")] string? ReviewLevel,
    [property: JsonPropertyName("previous_memorization")] StudentPreviousMemorizationDto? PreviousMemorization,
    [property: JsonPropertyName("attendance_preferences")] StudentAttendancePreferencesDto? AttendancePreferences,
    [property: JsonPropertyName("follow_up_plan")] StudentFollowUpPlanDto? FollowUpPlan,
    [property: JsonPropertyName("visibility")] string Visibility);

internal sealed record StudentPreviousMemorizationDto(
    [property: JsonPropertyName("memorization_level")] string? MemorizationLevel,
    [property: JsonPropertyName("review_level")] string? ReviewLevel,
    [property: JsonPropertyName("memorized_juz_count")] decimal? MemorizedJuzCount,
    [property: JsonPropertyName("memorized_surah_ids")] IReadOnlyList<string>? MemorizedSurahIds,
    [property: JsonPropertyName("last_completed_unit")] StudentPlanDetailDto? LastCompletedUnit,
    [property: JsonPropertyName("previous_teacher_notes")] string? PreviousTeacherNotes,
    [property: JsonPropertyName("stop_reasons")] string? StopReasons);

internal sealed record StudentAttendancePreferencesDto(
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("weekly_slots")] IReadOnlyList<StudentWeeklyAvailabilitySlotDto> WeeklySlots,
    [property: JsonPropertyName("preferred_session_duration_minutes")] int? PreferredSessionDurationMinutes);

internal sealed record StudentWeeklyAvailabilitySlotDto(
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("preferred")] bool Preferred);

internal sealed record StudentFollowUpPlanDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("created_by_user_id")] Guid CreatedByUserId,
    [property: JsonPropertyName("source_registration_request_id")] Guid? SourceRegistrationRequestId,
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("details")] IReadOnlyList<StudentPlanDetailDto> Details,
    [property: JsonPropertyName("attendance_preferences")] StudentAttendancePreferencesDto AttendancePreferences,
    [property: JsonPropertyName("starts_on")] DateOnly? StartsOn,
    [property: JsonPropertyName("ends_on")] DateOnly? EndsOn,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("approved_by_user_id")] Guid? ApprovedByUserId,
    [property: JsonPropertyName("approved_at")] DateTimeOffset? ApprovedAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

internal sealed record StudentPlanDetailDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("sort_order")] int SortOrder,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

[JsonConverter(typeof(UpdateStudentProfileRequestDtoJsonConverter))]
internal sealed record UpdateStudentProfileRequestDto(
    bool IncludeName,
    string? Name,
    bool IncludeBirthDate,
    DateOnly? BirthDate,
    bool IncludeGender,
    string? Gender,
    bool IncludeCountry,
    string? Country,
    bool IncludeCity,
    string? City,
    bool IncludeResidence,
    string? Residence,
    bool IncludePhone,
    string? Phone,
    bool IncludePhoneZone,
    string? PhoneZone,
    bool IncludeWhatsappPhone,
    string? WhatsappPhone,
    bool IncludeWhatsappZone,
    string? WhatsappZone,
    bool IncludeMemorizationLevel,
    string? MemorizationLevel,
    bool IncludeReviewLevel,
    string? ReviewLevel,
    bool IncludePreviousMemorization,
    StudentPreviousMemorizationRequestDto? PreviousMemorization,
    bool IncludeAttendancePreferences,
    StudentAttendancePreferencesRequestDto? AttendancePreferences,
    bool IncludeFollowUpPlan,
    StudentFollowUpPlanInputDto? FollowUpPlan,
    bool IncludeBio,
    string? Bio);

internal sealed record StudentPreviousMemorizationRequestDto(
    [property: JsonPropertyName("memorization_level")] string? MemorizationLevel,
    [property: JsonPropertyName("review_level")] string? ReviewLevel,
    [property: JsonPropertyName("memorized_juz_count")] decimal? MemorizedJuzCount,
    [property: JsonPropertyName("memorized_surah_ids")] IReadOnlyList<string> MemorizedSurahIds,
    [property: JsonPropertyName("last_completed_unit")] StudentPlanDetailDto? LastCompletedUnit,
    [property: JsonPropertyName("previous_teacher_notes")] string? PreviousTeacherNotes,
    [property: JsonPropertyName("stop_reasons")] string? StopReasons);

internal sealed record StudentAttendancePreferencesRequestDto(
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("weekly_slots")] IReadOnlyList<StudentWeeklyAvailabilitySlotDto> WeeklySlots,
    [property: JsonPropertyName("preferred_session_duration_minutes")] int? PreferredSessionDurationMinutes);

internal sealed record StudentFollowUpPlanInputDto(
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("details")] IReadOnlyList<StudentPlanDetailInputDto> Details,
    [property: JsonPropertyName("starts_on")] DateOnly? StartsOn,
    [property: JsonPropertyName("ends_on")] DateOnly? EndsOn);

internal sealed record StudentPlanDetailInputDto(
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("notes")] string? Notes);

internal sealed class UpdateStudentProfileRequestDtoJsonConverter : JsonConverter<UpdateStudentProfileRequestDto>
{
    public override UpdateStudentProfileRequestDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException("طلبات تحديث الملف التفصيلي تُسلسل فقط ولا تُقرأ من الخادم.");

    public override void Write(Utf8JsonWriter writer, UpdateStudentProfileRequestDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WriteOptional(writer, "name", value.IncludeName, value.Name, options);
        WriteOptional(writer, "birth_date", value.IncludeBirthDate, value.BirthDate, options);
        WriteOptional(writer, "gender", value.IncludeGender, value.Gender, options);
        WriteOptional(writer, "country", value.IncludeCountry, value.Country, options);
        WriteOptional(writer, "city", value.IncludeCity, value.City, options);
        WriteOptional(writer, "residence", value.IncludeResidence, value.Residence, options);
        WriteOptional(writer, "phone", value.IncludePhone, value.Phone, options);
        WriteOptional(writer, "phone_zone", value.IncludePhoneZone, value.PhoneZone, options);
        WriteOptional(writer, "whatsapp_phone", value.IncludeWhatsappPhone, value.WhatsappPhone, options);
        WriteOptional(writer, "whatsapp_zone", value.IncludeWhatsappZone, value.WhatsappZone, options);
        WriteOptional(writer, "memorization_level", value.IncludeMemorizationLevel, value.MemorizationLevel, options);
        WriteOptional(writer, "review_level", value.IncludeReviewLevel, value.ReviewLevel, options);
        WriteOptional(writer, "previous_memorization", value.IncludePreviousMemorization, value.PreviousMemorization, options);
        WriteOptional(writer, "attendance_preferences", value.IncludeAttendancePreferences, value.AttendancePreferences, options);
        WriteOptional(writer, "follow_up_plan", value.IncludeFollowUpPlan, value.FollowUpPlan, options);
        WriteOptional(writer, "bio", value.IncludeBio, value.Bio, options);
        writer.WriteEndObject();
    }

    private static void WriteOptional<T>(Utf8JsonWriter writer, string propertyName, bool include, T? value, JsonSerializerOptions options)
    {
        if (!include)
        {
            return;
        }

        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }
}
