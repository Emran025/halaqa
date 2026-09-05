namespace Halaqa.Desktop.Features.FollowUp.Domain.Entities;

public static class FollowUpSchedulePolicy
{
    public static bool IsScheduledOn(FollowUpPlan? plan, DateOnly date)
    {
        if (plan is null || !string.Equals(plan.Status, "active", StringComparison.OrdinalIgnoreCase) || plan.Details.Count == 0)
        {
            return false;
        }

        if (plan.StartsOn is { } startsOn && date < startsOn)
        {
            return false;
        }

        if (plan.EndsOn is { } endsOn && date > endsOn)
        {
            return false;
        }

        return plan.Frequency switch
        {
            FollowUpFrequency.Daily => true,
            FollowUpFrequency.OnceAWeek or FollowUpFrequency.TwiceAWeek or FollowUpFrequency.ThriceAWeek =>
                plan.AttendancePreferences.WeeklySlots.Any(slot => slot.DayOfWeek == (int)date.DayOfWeek),
            _ => false
        };
    }

    public static bool HasTaskType(FollowUpPlan? plan, FollowUpTaskType taskType) =>
        plan?.Details.Any(detail => detail.TaskType == taskType) == true;
}
