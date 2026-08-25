using Halaqa.Desktop.Features.Auth.Data.Models;
using Halaqa.Desktop.Features.Auth.Domain.Entities;

namespace Halaqa.Desktop.Features.Auth.Data.Mappers;

internal static class RegistrationMapper
{
    public static StudentRegistrationRequestDto ToDto(StudentRegistrationCommand command) => new(
        command.ClientOperationId,
        command.Name,
        command.Username,
        command.Email,
        command.Password,
        command.PasswordConfirmation,
        ToContract(command.Gender),
        command.BirthDate,
        command.Country,
        command.City,
        command.Residence,
        command.Phone,
        command.PhoneZone,
        command.WhatsappPhone,
        command.WhatsappZone,
        command.MemorizationLevel,
        command.ReviewLevel,
        new AttendancePreferencesDto(
            command.AttendancePreferences.Timezone,
            command.AttendancePreferences.WeeklySlots.Select(slot => new WeeklyAvailabilitySlotDto(slot.DayOfWeek, slot.From, slot.To, slot.Preferred)).ToArray(),
            command.AttendancePreferences.PreferredSessionDurationMinutes),
        new FollowUpPlanInputDto(
            ToContract(command.FollowUpPlan.Frequency),
            command.FollowUpPlan.Details.Select(detail => new PlanDetailInputDto(ToContract(detail.TaskType), detail.Unit, detail.Amount, detail.Notes)).ToArray(),
            command.FollowUpPlan.StartsOn,
            command.FollowUpPlan.EndsOn),
        command.TeacherCode,
        command.ProfileBio);

    public static TeacherRegistrationRequestDto ToDto(TeacherRegistrationCommand command) => new(
        command.ClientOperationId,
        command.Name,
        command.Username,
        command.Email,
        command.Password,
        command.PasswordConfirmation,
        ToContract(command.Gender),
        command.BirthDate,
        command.Country,
        command.City,
        command.Residence,
        command.Phone,
        command.PhoneZone,
        command.WhatsappPhone,
        command.WhatsappZone,
        command.Qualification,
        command.ExperienceYears,
        command.Bio,
        command.AvailableTime,
        command.MaxHalaqas);

    private static string ToContract(Gender gender) => gender == Gender.Male ? "male" : "female";
    private static string ToContract(FollowUpFrequency frequency) => frequency switch
    {
        FollowUpFrequency.Daily => "daily",
        FollowUpFrequency.OnceAWeek => "onceAWeek",
        FollowUpFrequency.TwiceAWeek => "twiceAWeek",
        FollowUpFrequency.ThriceAWeek => "thriceAWeek",
        _ => throw new ArgumentOutOfRangeException(nameof(frequency))
    };
    private static string ToContract(PlanTaskType taskType) => taskType switch
    {
        PlanTaskType.Memorization => "memorization",
        PlanTaskType.Review => "review",
        PlanTaskType.Recitation => "recitation",
        _ => throw new ArgumentOutOfRangeException(nameof(taskType))
    };
}
