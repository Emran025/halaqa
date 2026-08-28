using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;

public sealed partial class FollowUpViewModel : ObservableObject
{
    private readonly GetFollowUpPlanUseCase _getPlanUseCase;
    private readonly UpdateFollowUpPlanUseCase _updatePlanUseCase;
    private readonly GetAvailabilityUseCase _getAvailabilityUseCase;
    private readonly UpdateAvailabilityUseCase _updateAvailabilityUseCase;
    private readonly ListFollowUpItemsUseCase _listItemsUseCase;
    private readonly CompleteFollowUpItemUseCase _completeItemUseCase;
    private readonly SkipFollowUpItemUseCase _skipItemUseCase;
    private readonly RescheduleFollowUpItemUseCase _rescheduleItemUseCase;
    private readonly ListStudentTrackingsUseCase _listTrackingsUseCase;

    public FollowUpViewModel(
        GetFollowUpPlanUseCase getPlanUseCase,
        UpdateFollowUpPlanUseCase updatePlanUseCase,
        GetAvailabilityUseCase getAvailabilityUseCase,
        UpdateAvailabilityUseCase updateAvailabilityUseCase,
        ListFollowUpItemsUseCase listItemsUseCase,
        CompleteFollowUpItemUseCase completeItemUseCase,
        SkipFollowUpItemUseCase skipItemUseCase,
        RescheduleFollowUpItemUseCase rescheduleItemUseCase,
        ListStudentTrackingsUseCase listTrackingsUseCase)
    {
        _getPlanUseCase = getPlanUseCase;
        _updatePlanUseCase = updatePlanUseCase;
        _getAvailabilityUseCase = getAvailabilityUseCase;
        _updateAvailabilityUseCase = updateAvailabilityUseCase;
        _listItemsUseCase = listItemsUseCase;
        _completeItemUseCase = completeItemUseCase;
        _skipItemUseCase = skipItemUseCase;
        _rescheduleItemUseCase = rescheduleItemUseCase;
        _listTrackingsUseCase = listTrackingsUseCase;
    }

    public ObservableCollection<FollowUpItem> Items { get; } = new();
    public ObservableCollection<TrackingItem> Trackings { get; } = new();
    public ObservableCollection<FollowUpPlanDetailEditor> PlanDetails { get; } = new();
    public ObservableCollection<FollowUpAvailabilitySlotEditor> WeeklySlots { get; } = new();
    public IReadOnlyList<LocalizedOption<FollowUpFrequency>> FrequencyOptions { get; } = new[]
    {
        new LocalizedOption<FollowUpFrequency>(FollowUpFrequency.Daily, "يومياً"),
        new LocalizedOption<FollowUpFrequency>(FollowUpFrequency.OnceAWeek, "مرة أسبوعياً"),
        new LocalizedOption<FollowUpFrequency>(FollowUpFrequency.TwiceAWeek, "مرتان أسبوعياً"),
        new LocalizedOption<FollowUpFrequency>(FollowUpFrequency.ThriceAWeek, "ثلاث مرات أسبوعياً")
    };

    public IReadOnlyList<LocalizedOption<FollowUpTaskType>> TaskTypeOptions { get; } = new[]
    {
        new LocalizedOption<FollowUpTaskType>(FollowUpTaskType.Memorization, "حفظ"),
        new LocalizedOption<FollowUpTaskType>(FollowUpTaskType.Review, "مراجعة"),
        new LocalizedOption<FollowUpTaskType>(FollowUpTaskType.Recitation, "تلاوة")
    };

    public IReadOnlyList<LocalizedOption<FollowUpUnit>> UnitOptions { get; } = new[]
    {
        new LocalizedOption<FollowUpUnit>(FollowUpUnit.Page, "صفحة"),
        new LocalizedOption<FollowUpUnit>(FollowUpUnit.Juz, "جزء"),
        new LocalizedOption<FollowUpUnit>(FollowUpUnit.Hizb, "حزب"),
        new LocalizedOption<FollowUpUnit>(FollowUpUnit.HalfHizb, "نصف حزب"),
        new LocalizedOption<FollowUpUnit>(FollowUpUnit.QuarterHizb, "ربع حزب")
    };

    public IReadOnlyList<LocalizedOption<int>> WeekDayOptions { get; } = new[]
    {
        new LocalizedOption<int>(0, "الأحد"),
        new LocalizedOption<int>(1, "الاثنين"),
        new LocalizedOption<int>(2, "الثلاثاء"),
        new LocalizedOption<int>(3, "الأربعاء"),
        new LocalizedOption<int>(4, "الخميس"),
        new LocalizedOption<int>(5, "الجمعة"),
        new LocalizedOption<int>(6, "السبت")
    };

    [ObservableProperty] private Guid _studentId;
    [ObservableProperty] private FollowUpPlan? _plan;
    [ObservableProperty] private AttendancePreferences? _availability;
    [ObservableProperty] private FollowUpItem? _selectedItem;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private int _itemsCurrentPage = 1;
    [ObservableProperty] private int _itemsLastPage = 1;
    [ObservableProperty] private int _trackingsCurrentPage = 1;
    [ObservableProperty] private int _trackingsLastPage = 1;

    [ObservableProperty] private FollowUpFrequency _frequency = FollowUpFrequency.Daily;
    [ObservableProperty] private string? _startsOn;
    [ObservableProperty] private string? _endsOn;
    [ObservableProperty] private string _timezone = "UTC";
    [ObservableProperty] private string? _preferredSessionDurationMinutes = "30";
    [ObservableProperty] private string? _skipReason;
    [ObservableProperty] private string? _rescheduledAt;
    [ObservableProperty] private string? _rescheduleReason;

    public event EventHandler? BackRequested;

    public void Initialize(Guid studentId)
    {
        StudentId = studentId;
        Items.Clear();
        Trackings.Clear();
        Plan = null;
        Availability = null;
        PlanDetails.Clear();
        PlanDetails.Add(new FollowUpPlanDetailEditor());
        WeeklySlots.Clear();
        WeeklySlots.Add(new FollowUpAvailabilitySlotEditor());
        SelectedItem = null;
        ItemsCurrentPage = 1;
        ItemsLastPage = 1;
        TrackingsCurrentPage = 1;
        TrackingsLastPage = 1;
        ClearFeedback();
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadAllAsync();

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task RefreshItemsAsync() => await LoadItemsAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadNextItemsPageAsync()
    {
        if (ItemsCurrentPage < ItemsLastPage)
        {
            await LoadItemsAsync(ItemsCurrentPage + 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadPreviousItemsPageAsync()
    {
        if (ItemsCurrentPage > 1)
        {
            await LoadItemsAsync(ItemsCurrentPage - 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSavePlan))]
    private async Task SavePlanAsync()
    {
        if (!TryReadPlan(out var command, out var error))
        {
            SetLocalFailure(error!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _updatePlanUseCase.ExecuteAsync(command);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            ApplyPlan(result.Value);
            Message = "تم حفظ خطة المتابعة الرسمية.";
            await LoadItemsAsync(1);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditPlanDetails))]
    private void AddPlanDetail()
    {
        PlanDetails.Add(new FollowUpPlanDetailEditor());
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanRemovePlanDetail))]
    private void RemovePlanDetail(FollowUpPlanDetailEditor? detail)
    {
        if (detail is not null && PlanDetails.Count > 1)
        {
            PlanDetails.Remove(detail);
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditAvailabilitySlots))]
    private void AddAvailabilitySlot()
    {
        WeeklySlots.Add(new FollowUpAvailabilitySlotEditor());
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveAvailabilitySlot))]
    private void RemoveAvailabilitySlot(FollowUpAvailabilitySlotEditor? slot)
    {
        if (slot is not null && WeeklySlots.Count > 1)
        {
            WeeklySlots.Remove(slot);
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveAvailability))]
    private async Task SaveAvailabilityAsync()
    {
        if (!TryReadAvailability(out var command, out var error))
        {
            SetLocalFailure(error!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _updateAvailabilityUseCase.ExecuteAsync(command);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Availability = result.Value;
            ApplyAvailability(result.Value);
            Message = "تم حفظ أوقات الحضور الرسمية.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanActOnItem))]
    private async Task CompleteSelectedItemAsync()
    {
        var item = SelectedItem!;
        await UpdateItemAsync(() => _completeItemUseCase.ExecuteAsync(item.Id, Guid.NewGuid()), "تم تعليم عنصر المتابعة كمكتمل.");
    }

    [RelayCommand(CanExecute = nameof(CanActOnItem))]
    private async Task SkipSelectedItemAsync()
    {
        var item = SelectedItem!;
        if (string.IsNullOrWhiteSpace(SkipReason))
        {
            SetLocalFailure("اكتب سبب التجاوز قبل حفظه.");
            return;
        }

        await UpdateItemAsync(() => _skipItemUseCase.ExecuteAsync(item.Id, SkipReason.Trim(), Guid.NewGuid()), "تم تجاوز عنصر المتابعة مع تسجيل السبب.");
    }

    [RelayCommand(CanExecute = nameof(CanActOnItem))]
    private async Task RescheduleSelectedItemAsync()
    {
        var item = SelectedItem!;
        if (!DateTimeOffset.TryParse(RescheduledAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var scheduledAt))
        {
            SetLocalFailure("أدخل موعد إعادة الجدولة بصيغة تاريخ ووقت صالحة.");
            return;
        }

        await UpdateItemAsync(
            () => _rescheduleItemUseCase.ExecuteAsync(new RescheduleFollowUpItemCommand(item.Id, scheduledAt, Timezone, RescheduleReason, Guid.NewGuid())),
            "تمت إعادة جدولة عنصر المتابعة.");
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanLoad() => !IsBusy && StudentId != Guid.Empty;
    private bool CanSavePlan() => CanLoad() && PlanDetails.Count > 0;
    private bool CanSaveAvailability() => CanLoad() && WeeklySlots.Count > 0;
    private bool CanEditPlanDetails() => CanLoad();
    private bool CanRemovePlanDetail(FollowUpPlanDetailEditor? detail) => CanLoad() && detail is not null && PlanDetails.Count > 1;
    private bool CanEditAvailabilitySlots() => CanLoad();
    private bool CanRemoveAvailabilitySlot(FollowUpAvailabilitySlotEditor? slot) => CanLoad() && slot is not null && WeeklySlots.Count > 1;
    private bool CanActOnItem() => CanLoad() && SelectedItem is not null;

    private async Task LoadAllAsync()
    {
        if (StudentId == Guid.Empty)
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var planResult = await _getPlanUseCase.ExecuteAsync(StudentId);
            if (planResult.IsSuccess && planResult.Value is not null)
            {
                ApplyPlan(planResult.Value);
            }
            else
            {
                SetFailure(planResult.Error);
            }

            var availabilityResult = await _getAvailabilityUseCase.ExecuteAsync(StudentId);
            if (availabilityResult.IsSuccess && availabilityResult.Value is not null)
            {
                Availability = availabilityResult.Value;
                ApplyAvailability(availabilityResult.Value);
            }
            else if (!IsError)
            {
                SetFailure(availabilityResult.Error);
            }

            await LoadItemsAsync(1, keepBusy: true);
            await LoadTrackingsAsync(1, keepBusy: true);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private async Task LoadItemsAsync(int page, bool keepBusy = false)
    {
        if (!keepBusy)
        {
            IsBusy = true;
            ClearFeedback();
        }

        try
        {
            var result = await _listItemsUseCase.ExecuteAsync(new FollowUpItemQuery(null, null, null, null, page, 20));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Items.Clear();
            foreach (var item in result.Value.Items)
            {
                Items.Add(item);
            }
            ItemsCurrentPage = result.Value.CurrentPage;
            ItemsLastPage = result.Value.LastPage;
        }
        finally
        {
            if (!keepBusy)
            {
                IsBusy = false;
                NotifyCommands();
            }
        }
    }

    private async Task LoadTrackingsAsync(int page, bool keepBusy = false)
    {
        if (!keepBusy)
        {
            IsBusy = true;
        }

        try
        {
            var result = await _listTrackingsUseCase.ExecuteAsync(StudentId, null, null, page, 20);
            if (!result.IsSuccess || result.Value is null)
            {
                if (!IsError)
                {
                    SetFailure(result.Error);
                }
                return;
            }

            Trackings.Clear();
            foreach (var tracking in result.Value.Items)
            {
                Trackings.Add(tracking);
            }
            TrackingsCurrentPage = result.Value.CurrentPage;
            TrackingsLastPage = result.Value.LastPage;
        }
        finally
        {
            if (!keepBusy)
            {
                IsBusy = false;
                NotifyCommands();
            }
        }
    }

    private async Task UpdateItemAsync(Func<Task<Result<FollowUpItem>>> operation, string successMessage)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await operation();
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            var index = Items.ToList().FindIndex(item => item.Id == result.Value.Id);
            if (index >= 0)
            {
                Items[index] = result.Value;
            }
            SelectedItem = result.Value;
            Message = successMessage;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool TryReadPlan(out UpdateFollowUpPlanCommand command, out string? error)
    {
        command = default!;
        error = null;
        var details = new List<PlanDetailDraft>();
        foreach (var detail in PlanDetails)
        {
            if (!detail.TryToDraft(out var draft) || draft is null)
            {
                error = "تحقق من نوع كل تفصيل ووحدته وكميته الإيجابية وملاحظاته (حتى 500 حرف).";
                return false;
            }

            details.Add(draft);
        }
        if (details.Count == 0)
        {
            error = "أضف تفصيلاً واحداً على الأقل للخطة.";
            return false;
        }
        if (!TryReadDate(StartsOn, out var startsOn) || !TryReadDate(EndsOn, out var endsOn))
        {
            error = "أدخل تاريخ البداية والنهاية بصيغة YYYY-MM-DD أو اتركه فارغاً.";
            return false;
        }
        if (startsOn is { } start && endsOn is { } end && end < start)
        {
            error = "لا يمكن أن يسبق تاريخ النهاية تاريخ البداية.";
            return false;
        }

        command = new UpdateFollowUpPlanCommand(StudentId, Frequency, details, startsOn, endsOn);
        return true;
    }

    private bool TryReadAvailability(out UpdateAvailabilityCommand command, out string? error)
    {
        command = default!;
        error = null;
        if (string.IsNullOrWhiteSpace(Timezone) || !TryReadDuration(out var duration))
        {
            error = "تحقق من المنطقة الزمنية والمدة بين 10 و180 دقيقة.";
            return false;
        }

        var slots = new List<WeeklyAvailabilitySlot>();
        foreach (var editor in WeeklySlots)
        {
            if (!editor.TryToDomain(out var slot) || slot is null)
            {
                error = "تحقق من كل نطاق حضور: اليوم بين 0 و6، والأوقات بصيغة HH:mm، ووقت النهاية بعد البداية.";
                return false;
            }

            slots.Add(slot);
        }
        if (slots.Count == 0)
        {
            error = "أضف نطاق حضور أسبوعياً واحداً على الأقل.";
            return false;
        }

        command = new UpdateAvailabilityCommand(StudentId, new AttendancePreferences(Timezone.Trim(), slots, duration));
        return true;
    }

    private static bool TryReadDate(string? value, out DateOnly? date)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            date = null;
            return true;
        }
        var parsed = DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
        date = parsed ? result : null;
        return parsed;
    }

    private bool TryReadDuration(out int? duration)
    {
        if (string.IsNullOrWhiteSpace(PreferredSessionDurationMinutes))
        {
            duration = null;
            return true;
        }
        if (int.TryParse(PreferredSessionDurationMinutes, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value is >= 10 and <= 180)
        {
            duration = value;
            return true;
        }
        duration = null;
        return false;
    }

    private void ApplyPlan(FollowUpPlan value)
    {
        Plan = value;
        Frequency = value.Frequency;
        Timezone = value.Timezone;
        StartsOn = value.StartsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        EndsOn = value.EndsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        ReplaceEditors(PlanDetails, value.Details.OrderBy(item => item.SortOrder).Select(FollowUpPlanDetailEditor.FromDomain));
    }

    private void ApplyAvailability(AttendancePreferences value)
    {
        Timezone = value.Timezone;
        PreferredSessionDurationMinutes = value.PreferredSessionDurationMinutes?.ToString(CultureInfo.InvariantCulture);
        ReplaceEditors(WeeklySlots, value.WeeklySlots.Select(FollowUpAvailabilitySlotEditor.FromDomain));
    }

    private static void ReplaceEditors<TEditor>(ObservableCollection<TEditor> target, IEnumerable<TEditor> values)
    {
        target.Clear();
        foreach (var value in values)
        {
            target.Add(value);
        }
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تنفيذ عملية المتابعة.";
    }

    private void SetLocalFailure(string message)
    {
        IsError = true;
        Message = message;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        RefreshItemsCommand.NotifyCanExecuteChanged();
        LoadNextItemsPageCommand.NotifyCanExecuteChanged();
        LoadPreviousItemsPageCommand.NotifyCanExecuteChanged();
        SavePlanCommand.NotifyCanExecuteChanged();
        AddPlanDetailCommand.NotifyCanExecuteChanged();
        RemovePlanDetailCommand.NotifyCanExecuteChanged();
        SaveAvailabilityCommand.NotifyCanExecuteChanged();
        AddAvailabilitySlotCommand.NotifyCanExecuteChanged();
        RemoveAvailabilitySlotCommand.NotifyCanExecuteChanged();
        CompleteSelectedItemCommand.NotifyCanExecuteChanged();
        SkipSelectedItemCommand.NotifyCanExecuteChanged();
        RescheduleSelectedItemCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemChanged(FollowUpItem? value)
    {
        SkipReason = value?.SkipReason;
        RescheduledAt = value?.ScheduledFor.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);
        RescheduleReason = null;
        NotifyCommands();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
