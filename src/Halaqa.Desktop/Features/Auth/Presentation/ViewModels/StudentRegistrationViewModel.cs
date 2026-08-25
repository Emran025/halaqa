using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class StudentRegistrationViewModel(RegisterStudentUseCase registerStudentUseCase) : ObservableObject
{
    [ObservableProperty] private int _step = 1;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirmation = string.Empty;
    [ObservableProperty] private Gender _gender = Gender.Male;
    [ObservableProperty] private DateTime _birthDate = DateTime.Today.AddYears(-12);
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _phoneZone = string.Empty;
    [ObservableProperty] private string _timezone = "Asia/Riyadh";
    [ObservableProperty] private int _attendanceDay = 0;
    [ObservableProperty] private string _attendanceFrom = "18:00";
    [ObservableProperty] private string _attendanceTo = "19:00";
    [ObservableProperty] private FollowUpFrequency _frequency = FollowUpFrequency.Daily;
    [ObservableProperty] private PlanTaskType _taskType = PlanTaskType.Memorization;
    [ObservableProperty] private string _planUnit = "page";
    [ObservableProperty] private decimal _planAmount = 1;
    [ObservableProperty] private string? _teacherCode;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;

    public Array Genders => Enum.GetValues(typeof(Gender));
    public Array Frequencies => Enum.GetValues(typeof(FollowUpFrequency));
    public Array TaskTypes => Enum.GetValues(typeof(PlanTaskType));
    public bool IsFirstStep => Step == 1;
    public bool IsSecondStep => Step == 2;
    public bool IsThirdStep => Step == 3;

    partial void OnStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsSecondStep));
        OnPropertyChanged(nameof(IsThirdStep));
        PreviousCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void Previous() => Step--;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() => Step++;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        IsBusy = true;
        IsError = false;
        Message = null;
        SubmitCommand.NotifyCanExecuteChanged();
        try
        {
            var command = new StudentRegistrationCommand(
                Guid.NewGuid(), Name, null, Email, Password, PasswordConfirmation, Gender, DateOnly.FromDateTime(BirthDate),
                Country, City, null, Phone, PhoneZone, null, null, null, null,
                new AttendancePreferences(Timezone, [new WeeklyAvailabilitySlot(AttendanceDay, AttendanceFrom, AttendanceTo, true)], 30),
                new FollowUpPlan(Frequency, [new FollowUpPlanDetail(TaskType, PlanUnit, PlanAmount, null)], DateOnly.FromDateTime(DateTime.Today), null),
                TeacherCode, null);
            var result = await registerStudentUseCase.ExecuteAsync(command);
            IsError = !result.IsSuccess;
            Message = result.IsSuccess ? "تم إنشاء الحساب بنجاح." : RenderError(result.Error);
        }
        finally
        {
            IsBusy = false;
            SubmitCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanGoPrevious() => Step > 1;
    private bool CanGoNext() => Step < 3 && !IsBusy;
    private bool CanSubmit() => Step == 3 && !IsBusy;

    private static string RenderError(AppError? error) => error?.Message ?? "تعذر إنشاء الحساب حالياً.";
}
