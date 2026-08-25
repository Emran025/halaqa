using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Presentation.ViewModels;

public sealed partial class TeacherProfileViewModel : ObservableObject
{
    private readonly GetCurrentTeacherProfileUseCase _getCurrentTeacherProfileUseCase;
    private readonly UpdateCurrentTeacherProfileUseCase _updateCurrentTeacherProfileUseCase;
    private TeacherProfile? _loadedProfile;

    public TeacherProfileViewModel(
        GetCurrentTeacherProfileUseCase getCurrentTeacherProfileUseCase,
        UpdateCurrentTeacherProfileUseCase updateCurrentTeacherProfileUseCase)
    {
        _getCurrentTeacherProfileUseCase = getCurrentTeacherProfileUseCase;
        _updateCurrentTeacherProfileUseCase = updateCurrentTeacherProfileUseCase;
    }

    public ObservableCollection<TeacherDocumentSummary> Documents { get; } = [];
    public ObservableCollection<TeacherHalaqaSummary> PublicHalaqas { get; } = [];
    public IReadOnlyList<string> GenderOptions { get; } = ["male", "female"];

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _teacherCode = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string? _birthDate;
    [ObservableProperty] private string? _gender;
    [ObservableProperty] private string? _country;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _residence;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _phoneZone;
    [ObservableProperty] private string? _whatsappPhone;
    [ObservableProperty] private string? _whatsappZone;
    [ObservableProperty] private string? _qualification;
    [ObservableProperty] private string? _experienceYears;
    [ObservableProperty] private string? _availableTime;
    [ObservableProperty] private string? _bio;
    [ObservableProperty] private string? _maxHalaqas;
    [ObservableProperty] private bool _capacityAvailable;
    [ObservableProperty] private int _activeHalaqaCount;
    [ObservableProperty] private bool _clearBirthDate;
    [ObservableProperty] private bool _clearGender;
    [ObservableProperty] private bool _clearCountry;
    [ObservableProperty] private bool _clearCity;
    [ObservableProperty] private bool _clearResidence;
    [ObservableProperty] private bool _clearPhone;
    [ObservableProperty] private bool _clearPhoneZone;
    [ObservableProperty] private bool _clearWhatsappPhone;
    [ObservableProperty] private bool _clearWhatsappZone;
    [ObservableProperty] private bool _clearQualification;
    [ObservableProperty] private bool _clearExperienceYears;
    [ObservableProperty] private bool _clearAvailableTime;
    [ObservableProperty] private bool _clearBio;
    [ObservableProperty] private bool _clearMaxHalaqas;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _nameError;
    [ObservableProperty] private string? _birthDateError;
    [ObservableProperty] private string? _genderError;
    [ObservableProperty] private string? _countryError;
    [ObservableProperty] private string? _cityError;
    [ObservableProperty] private string? _residenceError;
    [ObservableProperty] private string? _phoneError;
    [ObservableProperty] private string? _phoneZoneError;
    [ObservableProperty] private string? _whatsappPhoneError;
    [ObservableProperty] private string? _whatsappZoneError;
    [ObservableProperty] private string? _qualificationError;
    [ObservableProperty] private string? _experienceYearsError;
    [ObservableProperty] private string? _availableTimeError;
    [ObservableProperty] private string? _bioError;
    [ObservableProperty] private string? _maxHalaqasError;

    public event EventHandler? BackRequested;
    public event EventHandler? DocumentsRequested;
    public event EventHandler<TeacherProfile>? ProfileUpdated;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _getCurrentTeacherProfileUseCase.ExecuteAsync();
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            ApplyProfile(result.Value);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!TryCreateUpdateCommand(out var command, out var localError))
        {
            SetLocalFailure(localError!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _updateCurrentTeacherProfileUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            ApplyProfile(result.Value);
            MaxHalaqas = null;
            ClearMaxHalaqas = false;
            Message = "تم حفظ الملف التفصيلي للمعلم.";
            ProfileUpdated?.Invoke(this, result.Value);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void OpenDocuments() => DocumentsRequested?.Invoke(this, EventArgs.Empty);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(IsBusy))
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanLoad() => !IsBusy;
    private bool CanSave() => !IsBusy && TryCreateUpdateCommand(out _, out _);
    private bool CanNavigateBack() => !IsBusy;

    private void ApplyProfile(TeacherProfile profile)
    {
        _loadedProfile = profile;
        Name = profile.DisplayName;
        TeacherCode = profile.TeacherCode;
        Email = profile.Email ?? string.Empty;
        BirthDate = profile.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        Gender = ToContractValue(profile.Gender);
        Country = profile.Country;
        City = profile.City;
        Residence = profile.Residence;
        Phone = profile.Phone;
        PhoneZone = profile.PhoneZone;
        WhatsappPhone = profile.WhatsappPhone;
        WhatsappZone = profile.WhatsappZone;
        Qualification = profile.Qualification;
        ExperienceYears = profile.ExperienceYears?.ToString(CultureInfo.InvariantCulture);
        AvailableTime = profile.AvailableTime;
        Bio = profile.Bio;
        MaxHalaqas = null;
        CapacityAvailable = profile.CapacityAvailable;
        ActiveHalaqaCount = profile.ActiveHalaqaCount;
        ClearBirthDate = false;
        ClearGender = false;
        ClearCountry = false;
        ClearCity = false;
        ClearResidence = false;
        ClearPhone = false;
        ClearPhoneZone = false;
        ClearWhatsappPhone = false;
        ClearWhatsappZone = false;
        ClearQualification = false;
        ClearExperienceYears = false;
        ClearAvailableTime = false;
        ClearBio = false;
        ClearMaxHalaqas = false;
        Replace(Documents, profile.Documents);
        Replace(PublicHalaqas, profile.PublicHalaqas);
    }

    private bool TryCreateUpdateCommand(out UpdateTeacherProfileCommand? command, out string? error)
    {
        command = null;
        error = null;
        var loaded = _loadedProfile;
        if (loaded is null)
        {
            error = "حمّل ملف المعلم أولاً قبل الحفظ.";
            return false;
        }

        if (!TryReadDate(BirthDate, out var birthDate, out error) ||
            !TryReadOptionalInteger(ExperienceYears, "سنوات الخبرة", 0, 80, out var experienceYears, out error) ||
            !TryReadOptionalInteger(MaxHalaqas, "الحد الأقصى للحلقات", 0, int.MaxValue, out var maxHalaqas, out error))
        {
            return false;
        }

        var gender = TryReadGender(Gender);
        if (!ClearGender && !string.IsNullOrWhiteSpace(Gender) && gender is null)
        {
            error = "اختر الجنس بصيغة صحيحة.";
            return false;
        }

        command = new UpdateTeacherProfileCommand(
            ChangedRequired(Name, loaded.DisplayName),
            ChangedDate(birthDate, loaded.BirthDate, ClearBirthDate),
            ChangedGender(gender, loaded.Gender, ClearGender),
            ChangedOptional(Country, loaded.Country, ClearCountry),
            ChangedOptional(City, loaded.City, ClearCity),
            ChangedOptional(Residence, loaded.Residence, ClearResidence),
            ChangedOptional(Phone, loaded.Phone, ClearPhone),
            ChangedOptional(PhoneZone, loaded.PhoneZone, ClearPhoneZone),
            ChangedOptional(WhatsappPhone, loaded.WhatsappPhone, ClearWhatsappPhone),
            ChangedOptional(WhatsappZone, loaded.WhatsappZone, ClearWhatsappZone),
            ChangedOptional(Qualification, loaded.Qualification, ClearQualification),
            ChangedInteger(experienceYears, loaded.ExperienceYears, ClearExperienceYears),
            ChangedOptional(AvailableTime, loaded.AvailableTime, ClearAvailableTime),
            ChangedOptional(Bio, loaded.Bio, ClearBio),
            ChangedMaxHalaqas(maxHalaqas, ClearMaxHalaqas));

        return true;
    }

    private static TeacherProfileUpdateField<string> ChangedRequired(string current, string loaded) =>
        !string.Equals(current.Trim(), loaded, StringComparison.Ordinal)
            ? TeacherProfileUpdateField<string>.Set(current.Trim())
            : TeacherProfileUpdateField<string>.Omit();

    private static TeacherProfileUpdateField<string> ChangedOptional(string? current, string? loaded, bool clear) =>
        clear
            ? TeacherProfileUpdateField<string>.Set(null)
            : !string.Equals(NormalizeOptional(current), loaded, StringComparison.Ordinal)
                ? TeacherProfileUpdateField<string>.Set(NormalizeOptional(current))
                : TeacherProfileUpdateField<string>.Omit();

    private static TeacherProfileUpdateField<DateOnly?> ChangedDate(DateOnly? current, DateOnly? loaded, bool clear) =>
        clear
            ? TeacherProfileUpdateField<DateOnly?>.Set(null)
            : current != loaded
                ? TeacherProfileUpdateField<DateOnly?>.Set(current)
                : TeacherProfileUpdateField<DateOnly?>.Omit();

    private static TeacherProfileUpdateField<TeacherGender?> ChangedGender(TeacherGender? current, TeacherGender loaded, bool clear) =>
        clear
            ? TeacherProfileUpdateField<TeacherGender?>.Set(null)
            : current is { } value && value != loaded
                ? TeacherProfileUpdateField<TeacherGender?>.Set(value)
                : TeacherProfileUpdateField<TeacherGender?>.Omit();

    private static TeacherProfileUpdateField<int?> ChangedInteger(int? current, int? loaded, bool clear) =>
        clear
            ? TeacherProfileUpdateField<int?>.Set(null)
            : current != loaded
                ? TeacherProfileUpdateField<int?>.Set(current)
                : TeacherProfileUpdateField<int?>.Omit();

    private static TeacherProfileUpdateField<int?> ChangedMaxHalaqas(int? current, bool clear) =>
        clear
            ? TeacherProfileUpdateField<int?>.Set(null)
            : current is { } value
                ? TeacherProfileUpdateField<int?>.Set(value)
                : TeacherProfileUpdateField<int?>.Omit();

    private static bool TryReadDate(string? text, out DateOnly? date, out string? error)
    {
        date = null;
        error = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            error = "أدخل تاريخ الميلاد بصيغة YYYY-MM-DD.";
            return false;
        }

        date = parsed;
        return true;
    }

    private static bool TryReadOptionalInteger(string? text, string label, int minimum, int maximum, out int? value, out string? error)
    {
        value = null;
        error = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed < minimum || parsed > maximum)
        {
            error = $"أدخل {label} كرقم بين {minimum} و{maximum}.";
            return false;
        }

        value = parsed;
        return true;
    }

    private static TeacherGender? TryReadGender(string? value) =>
        Enum.TryParse<TeacherGender>(value, ignoreCase: true, out var parsed) ? parsed : null;

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
        OpenDocumentsCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        NameError = null;
        BirthDateError = null;
        GenderError = null;
        CountryError = null;
        CityError = null;
        ResidenceError = null;
        PhoneError = null;
        PhoneZoneError = null;
        WhatsappPhoneError = null;
        WhatsappZoneError = null;
        QualificationError = null;
        ExperienceYearsError = null;
        AvailableTimeError = null;
        BioError = null;
        MaxHalaqasError = null;
    }

    private void SetLocalFailure(string message)
    {
        ClearFeedback();
        IsError = true;
        Message = message;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        if (error?.FieldErrors is { Count: > 0 } fieldErrors)
        {
            foreach (var fieldError in fieldErrors)
            {
                var fieldMessage = string.Join(" ", fieldError.Messages);
                switch (fieldError.Field)
                {
                    case "name": NameError = fieldMessage; break;
                    case "birth_date": BirthDateError = fieldMessage; break;
                    case "gender": GenderError = fieldMessage; break;
                    case "country": CountryError = fieldMessage; break;
                    case "city": CityError = fieldMessage; break;
                    case "residence": ResidenceError = fieldMessage; break;
                    case "phone": PhoneError = fieldMessage; break;
                    case "phone_zone": PhoneZoneError = fieldMessage; break;
                    case "whatsapp_phone": WhatsappPhoneError = fieldMessage; break;
                    case "whatsapp_zone": WhatsappZoneError = fieldMessage; break;
                    case "qualification": QualificationError = fieldMessage; break;
                    case "experience_years": ExperienceYearsError = fieldMessage; break;
                    case "available_time": AvailableTimeError = fieldMessage; break;
                    case "bio": BioError = fieldMessage; break;
                    case "max_halaqas": MaxHalaqasError = fieldMessage; break;
                }
            }
        }

        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }
}
