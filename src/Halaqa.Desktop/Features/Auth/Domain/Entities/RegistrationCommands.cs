namespace Halaqa.Desktop.Features.Auth.Domain.Entities;

public enum Gender
{
    Male,
    Female
}

public enum FollowUpFrequency
{
    Daily,
    OnceAWeek,
    TwiceAWeek,
    ThriceAWeek
}

public enum PlanTaskType
{
    Memorization,
    Review,
    Recitation
}

public sealed record WeeklyAvailabilitySlot(int DayOfWeek, string From, string To, bool Preferred);
public sealed record AttendancePreferences(string Timezone, IReadOnlyList<WeeklyAvailabilitySlot> WeeklySlots, int? PreferredSessionDurationMinutes);
public sealed record FollowUpPlanDetail(PlanTaskType TaskType, string Unit, decimal Amount, string? Notes);
public sealed record FollowUpPlan(FollowUpFrequency Frequency, IReadOnlyList<FollowUpPlanDetail> Details, DateOnly? StartsOn, DateOnly? EndsOn);

public sealed record StudentRegistrationCommand(
    Guid ClientOperationId,
    string Name,
    string? Username,
    string Email,
    string Password,
    string PasswordConfirmation,
    Gender Gender,
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
    AttendancePreferences AttendancePreferences,
    FollowUpPlan FollowUpPlan,
    string? TeacherCode,
    string? ProfileBio);

public sealed record TeacherRegistrationCommand(
    Guid ClientOperationId,
    string Name,
    string? Username,
    string Email,
    string Password,
    string PasswordConfirmation,
    Gender Gender,
    DateOnly BirthDate,
    string Country,
    string City,
    string? Residence,
    string Phone,
    string PhoneZone,
    string? WhatsappPhone,
    string? WhatsappZone,
    string Qualification,
    int ExperienceYears,
    string? Bio,
    string? AvailableTime,
    int? MaxHalaqas);
