using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class FollowUpPlanDetailEditor : ObservableObject
{
    [ObservableProperty] private FollowUpTaskType _taskType = FollowUpTaskType.Memorization;
    [ObservableProperty] private FollowUpUnit _unit = FollowUpUnit.Page;
    [ObservableProperty] private string _amount = "1";
    [ObservableProperty] private string? _notes;

    public static FollowUpPlanDetailEditor FromDomain(FollowUpPlanDetail value) => new()
    {
        TaskType = value.TaskType,
        Unit = value.Unit,
        Amount = value.Amount.ToString(CultureInfo.InvariantCulture),
        Notes = value.Notes
    };

    public bool TryToDraft(out PlanDetailDraft? value)
    {
        value = null;
        if (!decimal.TryParse(Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0 || Notes?.Length > 500)
        {
            return false;
        }

        value = new PlanDetailDraft(TaskType, Unit, amount, NormalizeOptional(Notes));
        return true;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed partial class FollowUpAvailabilitySlotEditor : ObservableObject
{
    [ObservableProperty] private int _dayOfWeek;
    [ObservableProperty] private string _from = "18:00";
    [ObservableProperty] private string _to = "18:30";
    [ObservableProperty] private bool _preferred = true;

    public static FollowUpAvailabilitySlotEditor FromDomain(WeeklyAvailabilitySlot value) => new()
    {
        DayOfWeek = value.DayOfWeek,
        From = value.From.ToString("HH:mm", CultureInfo.InvariantCulture),
        To = value.To.ToString("HH:mm", CultureInfo.InvariantCulture),
        Preferred = value.Preferred
    };

    public bool TryToDomain(out WeeklyAvailabilitySlot? value)
    {
        value = null;
        if (DayOfWeek is < 0 or > 6 ||
            !TimeOnly.TryParseExact(From, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) ||
            !TimeOnly.TryParseExact(To, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to) ||
            from >= to)
        {
            return false;
        }

        value = new WeeklyAvailabilitySlot(DayOfWeek, from, to, Preferred);
        return true;
    }
}
