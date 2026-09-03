﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;
using Halaqa.Desktop.Shared.Services;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class TeacherRegistrationViewModel : ObservableObject
{
    private readonly RegisterTeacherUseCase registerTeacherUseCase;
    private readonly ICountryService _countryService;

    public TeacherRegistrationViewModel(
        RegisterTeacherUseCase registerTeacherUseCase,
        ICountryService countryService)
    {
        this.registerTeacherUseCase = registerTeacherUseCase;
        _countryService = countryService;
        Countries = _countryService.GetAllCountries();
    }

    private readonly Guid _clientOperationId = Guid.NewGuid();

    [ObservableProperty] private int _step = 1;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirmation = string.Empty;
    [ObservableProperty] private Gender _gender = Gender.Male;
    [ObservableProperty] private DateTime _birthDate = DateTime.Today.AddYears(-21);
    [ObservableProperty] private CountryItem? _selectedCountry;
    [ObservableProperty] private CountryItem? _selectedPhoneZoneCountry;
    [ObservableProperty] private CountryItem? _selectedWhatsappZoneCountry;
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _phoneZone = string.Empty;
    [ObservableProperty] private string _whatsappPhone = string.Empty;
    [ObservableProperty] private string _whatsappZone = string.Empty;
    [ObservableProperty] private string _qualification = string.Empty;
    [ObservableProperty] private int _experienceYears;
    [ObservableProperty] private string? _bio;
    [ObservableProperty] private string? _availableTime;
    [ObservableProperty] private int? _maxHalaqas;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;

    public event EventHandler<AuthenticatedUser>? Registered;
    public event EventHandler? LoginRequested;

    public IReadOnlyList<CountryItem> Countries { get; }

    public IReadOnlyList<LocalizedOption<Gender>> Genders { get; } = new[]
    {
        new LocalizedOption<Gender>(Gender.Male, "ذكر"),
        new LocalizedOption<Gender>(Gender.Female, "أنثى")
    };

    partial void OnSelectedCountryChanged(CountryItem? value)
    {
        if (value != null)
        {
            Country = value.NameAr;
        }
    }

    partial void OnSelectedPhoneZoneCountryChanged(CountryItem? value)
    {
        if (value != null && !string.IsNullOrWhiteSpace(value.PhoneCode))
            PhoneZone = value.PhoneCode;
    }

    partial void OnSelectedWhatsappZoneCountryChanged(CountryItem? value)
    {
        if (value != null && !string.IsNullOrWhiteSpace(value.PhoneCode))
            WhatsappZone = value.PhoneCode;
    }

    public IReadOnlyList<LocalizedOption<string>> QualificationOptions { get; } = new[]
    {
        new LocalizedOption<string>("إجازة قرآنية", "إجازة قرآنية"),
        new LocalizedOption<string>("تعليم شرعي", "تعليم شرعي"),
        new LocalizedOption<string>("دبلوم", "دبلوم"),
        new LocalizedOption<string>("بكالوريوس", "بكالوريوس"),
        new LocalizedOption<string>("ماجستير", "ماجستير"),
        new LocalizedOption<string>("دكتوراه", "دكتوراه"),
        new LocalizedOption<string>("أخرى", "أخرى")
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
                DateOnly.FromDateTime(BirthDate), Country, City, null, Phone, PhoneZone, WhatsappPhone, WhatsappZone,
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

    [RelayCommand]
    private void OpenLogin() => LoginRequested?.Invoke(this, EventArgs.Empty);

    private bool CanGoPrevious() => Step > 1 && !IsBusy;
    private bool CanGoNext() => Step < 2 && !IsBusy;
    private bool CanSubmit() => Step == 2 && !IsBusy;

    private static string RenderError(AppError? error)
    {
        if (error is null)
            return "تعذر إنشاء حساب المعلم حالياً.";

        if (error.FieldErrors is { Count: > 0 })
        {
            var details = error.FieldErrors
                .SelectMany(field => field.Messages.Select(message => $"{field.Field}: {message}"))
                .ToArray();
            if (details.Length > 0)
                return string.Join(Environment.NewLine, details);
        }

        return error.Message;
    }
}
