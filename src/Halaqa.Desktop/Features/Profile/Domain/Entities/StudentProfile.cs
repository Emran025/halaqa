namespace Halaqa.Desktop.Features.Profile.Domain.Entities;

public enum StudentGender
{
    Male,
    Female
}

public enum StudentProfileVisibility
{
    Self,
    RelationshipVisible
}

public enum FollowUpFrequency
{
    Daily,
    OnceAWeek,
    TwiceAWeek,
    ThriceAWeek
}

public enum QuranTaskType
{
    Memorization,
    Review,
    Recitation
}

public enum QuranPlanUnit
{
    Juz,
    Hizb,
    HalfHizb,
    QuarterHizb,
    Page
}

public sealed record StudentPlanDetail(
    Guid Id,
    QuranTaskType TaskType,
    QuranPlanUnit Unit,
    decimal Amount,
    string? Notes,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record StudentPlanDetailDraft(
    QuranTaskType TaskType,
    QuranPlanUnit Unit,
    decimal Amount,
    string? Notes);

public sealed record StudentPreviousMemorization(
    string? MemorizationLevel,
    string? ReviewLevel,
    decimal? MemorizedJuzCount,
    IReadOnlyList<string> MemorizedSurahIds,
    StudentPlanDetail? LastCompletedUnit,
    string? PreviousTeacherNotes,
    string? StopReasons);

public sealed record StudentWeeklyAvailabilitySlot(
    int DayOfWeek,
    TimeOnly From,
    TimeOnly To,
    bool Preferred);

public sealed record StudentAttendancePreferences(
    string Timezone,
    IReadOnlyList<StudentWeeklyAvailabilitySlot> WeeklySlots,
    int? PreferredSessionDurationMinutes);

public sealed record StudentFollowUpPlanDraft(
    FollowUpFrequency Frequency,
    IReadOnlyList<StudentPlanDetailDraft> Details,
    DateOnly? StartsOn,
    DateOnly? EndsOn);

public sealed record StudentFollowUpPlan(
    Guid Id,
    Guid StudentId,
    Guid CreatedByUserId,
    Guid? SourceRegistrationRequestId,
    FollowUpFrequency Frequency,
    string Status,
    string Timezone,
    IReadOnlyList<StudentPlanDetail> Details,
    StudentAttendancePreferences AttendancePreferences,
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    int Version,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record StudentProfile(
    Guid Id,
    string Name,
    string Email,
    string Status,
    DateOnly? BirthDate,
    StudentGender Gender,
    string? Country,
    string? City,
    string? Residence,
    string? Phone,
    string? PhoneZone,
    string? WhatsappPhone,
    string? WhatsappZone,
    string? MemorizationLevel,
    string? ReviewLevel,
    StudentPreviousMemorization? PreviousMemorization,
    StudentAttendancePreferences? AttendancePreferences,
    StudentFollowUpPlan? FollowUpPlan,
    StudentProfileVisibility Visibility);

public sealed record StudentProfileUpdateField<T>(bool IsSpecified, T? Value)
{
    public static StudentProfileUpdateField<T> Omit() => new(false, default);
    public static StudentProfileUpdateField<T> Set(T? value) => new(true, value);
}

public sealed record UpdateStudentProfileCommand(
    StudentProfileUpdateField<string> Name,
    StudentProfileUpdateField<DateOnly?> BirthDate,
    StudentProfileUpdateField<StudentGender?> Gender,
    StudentProfileUpdateField<string> Country,
    StudentProfileUpdateField<string> City,
    StudentProfileUpdateField<string> Residence,
    StudentProfileUpdateField<string> Phone,
    StudentProfileUpdateField<string> PhoneZone,
    StudentProfileUpdateField<string> WhatsappPhone,
    StudentProfileUpdateField<string> WhatsappZone,
    StudentProfileUpdateField<string> MemorizationLevel,
    StudentProfileUpdateField<string> ReviewLevel,
    StudentProfileUpdateField<StudentPreviousMemorization> PreviousMemorization,
    StudentProfileUpdateField<StudentAttendancePreferences> AttendancePreferences,
    StudentProfileUpdateField<StudentFollowUpPlanDraft> FollowUpPlan,
    StudentProfileUpdateField<string> Bio)
{
    public bool HasChanges =>
        Name.IsSpecified ||
        BirthDate.IsSpecified ||
        Gender.IsSpecified ||
        Country.IsSpecified ||
        City.IsSpecified ||
        Residence.IsSpecified ||
        Phone.IsSpecified ||
        PhoneZone.IsSpecified ||
        WhatsappPhone.IsSpecified ||
        WhatsappZone.IsSpecified ||
        MemorizationLevel.IsSpecified ||
        ReviewLevel.IsSpecified ||
        PreviousMemorization.IsSpecified ||
        AttendancePreferences.IsSpecified ||
        FollowUpPlan.IsSpecified ||
        Bio.IsSpecified;
}
