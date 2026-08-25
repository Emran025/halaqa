using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Auth.Data.Models;

internal sealed record WeeklyAvailabilitySlotDto(
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("preferred")] bool Preferred);

internal sealed record AttendancePreferencesDto(
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("weekly_slots")] IReadOnlyList<WeeklyAvailabilitySlotDto> WeeklySlots,
    [property: JsonPropertyName("preferred_session_duration_minutes")] int? PreferredSessionDurationMinutes);

internal sealed record PlanDetailInputDto(
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("notes")] string? Notes);

internal sealed record FollowUpPlanInputDto(
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("details")] IReadOnlyList<PlanDetailInputDto> Details,
    [property: JsonPropertyName("starts_on")] DateOnly? StartsOn,
    [property: JsonPropertyName("ends_on")] DateOnly? EndsOn);

internal sealed record StudentRegistrationRequestDto(
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("password_confirmation")] string PasswordConfirmation,
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
    [property: JsonPropertyName("attendance_preferences")] AttendancePreferencesDto AttendancePreferences,
    [property: JsonPropertyName("follow_up_plan")] FollowUpPlanInputDto FollowUpPlan,
    [property: JsonPropertyName("teacher_code")] string? TeacherCode,
    [property: JsonPropertyName("profile_bio")] string? ProfileBio);

internal sealed record TeacherRegistrationRequestDto(
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("password_confirmation")] string PasswordConfirmation,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("birth_date")] DateOnly BirthDate,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("residence")] string? Residence,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("phone_zone")] string PhoneZone,
    [property: JsonPropertyName("whatsapp_phone")] string? WhatsappPhone,
    [property: JsonPropertyName("whatsapp_zone")] string? WhatsappZone,
    [property: JsonPropertyName("qualification")] string Qualification,
    [property: JsonPropertyName("experience_years")] int ExperienceYears,
    [property: JsonPropertyName("bio")] string? Bio,
    [property: JsonPropertyName("available_time")] string? AvailableTime,
    [property: JsonPropertyName("max_halaqas")] int? MaxHalaqas);

internal sealed record ForgotPasswordRequestDto([property: JsonPropertyName("email")] string Email);
internal sealed record ResetPasswordRequestDto(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("password_confirmation")] string PasswordConfirmation);
internal sealed record ChangePasswordRequestDto(
    [property: JsonPropertyName("current_password")] string CurrentPassword,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("password_confirmation")] string PasswordConfirmation);
