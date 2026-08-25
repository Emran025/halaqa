namespace Halaqa.Desktop.Features.Registrations.Domain.Entities;

public enum RegistrationGender
{
    Male,
    Female
}

public sealed record PublicHalaqa(
    Guid Id,
    string Name,
    string Status,
    RegistrationGender Gender,
    string Country,
    string Residence,
    int? AvailableCapacity);

public sealed record AvailableTeacher(
    Guid Id,
    string DisplayName,
    string TeacherCode,
    string? Avatar,
    RegistrationGender Gender,
    string Country,
    string City,
    string Qualification,
    int ExperienceYears,
    bool CapacityAvailable,
    string? Bio,
    int? ActiveHalaqaCount,
    IReadOnlyList<PublicHalaqa> PublicHalaqas);

public sealed record AvailableTeacherPage(
    IReadOnlyList<AvailableTeacher> Teachers,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record RegistrationApplicationProfile(
    RegistrationGender Gender,
    DateOnly BirthDate,
    string Country,
    string City,
    string? Residence,
    string Phone,
    string PhoneZone,
    string? WhatsappPhone,
    string? WhatsappZone,
    string? MemorizationLevel,
    string? ReviewLevel,
    string? Bio);

public sealed record RegistrationPreviousMemorization(
    string? MemorizationLevel,
    string? ReviewLevel,
    decimal? MemorizedJuzCount,
    IReadOnlyList<string> MemorizedSurahIds,
    string? PreviousTeacherNotes,
    string? StopReasons);

public sealed record RegistrationWeeklyAvailabilitySlot(
    int DayOfWeek,
    TimeOnly From,
    TimeOnly To,
    bool Preferred);

public sealed record RegistrationAttendancePreferences(
    string Timezone,
    IReadOnlyList<RegistrationWeeklyAvailabilitySlot> WeeklySlots,
    int? PreferredSessionDurationMinutes);

public sealed record RegistrationPlanDetail(
    string TaskType,
    string Unit,
    decimal Amount,
    string? Notes);

public sealed record RegistrationFollowUpPlan(
    string Frequency,
    IReadOnlyList<RegistrationPlanDetail> Details,
    DateOnly? StartsOn,
    DateOnly? EndsOn);

public sealed record CreateStudentRegistrationRequestCommand(
    string? TeacherCode,
    Guid? RequestedHalaqaId,
    string? Message,
    RegistrationApplicationProfile Profile,
    RegistrationPreviousMemorization? PreviousMemorization,
    RegistrationAttendancePreferences AttendancePreferences,
    RegistrationFollowUpPlan FollowUpPlan,
    Guid ClientOperationId);
