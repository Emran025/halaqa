using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class TeacherRegistrationViewModel : ObservableObject
{

    private readonly RegisterTeacherUseCase registerTeacherUseCase;


    public TeacherRegistrationViewModel(

        RegisterTeacherUseCase registerTeacherUseCase

    )

    {

        this.registerTeacherUseCase = registerTeacherUseCase;

    }

    private readonly Guid _clientOperationId = Guid.NewGuid();

    [ObservableProperty] private int _step = 1;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirmation = string.Empty;
    [ObservableProperty] private Gender _gender = Gender.Male;
    [ObservableProperty] private DateTime _birthDate = DateTime.Today.AddYears(-21);
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _phoneZone = string.Empty;
    [ObservableProperty] private string _qualification = string.Empty;
    [ObservableProperty] private int _experienceYears;
    [ObservableProperty] private string? _bio;
    [ObservableProperty] private string? _availableTime;
    [ObservableProperty] private int? _maxHalaqas;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;

    public event EventHandler<AuthenticatedUser>? Registered;

    public IReadOnlyList<LocalizedOption<Gender>> Genders { get; } = new[]
    {
        new LocalizedOption<Gender>(Gender.Male, "ذكر"),
        new LocalizedOption<Gender>(Gender.Female, "أنثى")
    };
    public bool IsFirstStep => Step == 1;
    public bool IsSecondStep => Step == 2;

    partial void OnStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsSecondStep));
        PreviousCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
        SubmitCommand.NotifyCanExecuteChanged();
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
            var command = new TeacherRegistrationCommand(
                _clientOperationId, Name, null, Email, Password, PasswordConfirmation, Gender,
                DateOnly.FromDateTime(BirthDate), Country, City, null, Phone, PhoneZone, null, null,
                Qualification, ExperienceYears, Bio, AvailableTime, MaxHalaqas);
            var result = await registerTeacherUseCase.ExecuteAsync(command);
            IsError = !result.IsSuccess;
            Message = result.IsSuccess ? "تم إنشاء حساب المعلم بنجاح." : RenderError(result.Error);
            if (result.IsSuccess && result.Value is not null)
            {
                Registered?.Invoke(this, result.Value);
            }
        }
        finally
        {
            IsBusy = false;
            SubmitCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanGoPrevious() => Step > 1 && !IsBusy;
    private bool CanGoNext() => Step < 2 && !IsBusy;
    private bool CanSubmit() => Step == 2 && !IsBusy;

    private static string RenderError(AppError? error) => error?.Message ?? "تعذر إنشاء حساب المعلم حالياً.";
}
