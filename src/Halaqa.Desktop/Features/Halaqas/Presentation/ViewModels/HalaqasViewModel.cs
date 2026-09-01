using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Halaqas.Presentation.ViewModels;

public sealed partial class HalaqasViewModel : ObservableObject
{
    private readonly ListHalaqasUseCase _listHalaqasUseCase;
    private readonly CreateHalaqaUseCase _createHalaqaUseCase;
    private readonly UpdateHalaqaUseCase _updateHalaqaUseCase;
    private readonly ActivateHalaqaUseCase _activateHalaqaUseCase;
    private readonly DeactivateHalaqaUseCase _deactivateHalaqaUseCase;

    public HalaqasViewModel(
        ListHalaqasUseCase listHalaqasUseCase,
        CreateHalaqaUseCase createHalaqaUseCase,
        UpdateHalaqaUseCase updateHalaqaUseCase,
        ActivateHalaqaUseCase activateHalaqaUseCase,
        DeactivateHalaqaUseCase deactivateHalaqaUseCase)
    {
        _listHalaqasUseCase = listHalaqasUseCase;
        _createHalaqaUseCase = createHalaqaUseCase;
        _updateHalaqaUseCase = updateHalaqaUseCase;
        _activateHalaqaUseCase = activateHalaqaUseCase;
        _deactivateHalaqaUseCase = deactivateHalaqaUseCase;
    }

    public ObservableCollection<HalaqaItem> Halaqas { get; } = new();
    public IReadOnlyList<LocalizedOption<string>> GenderOptions { get; } = new[]
    {
        new LocalizedOption<string>("male", "ذكور"),
        new LocalizedOption<string>("female", "إناث")
    };

    [ObservableProperty] private HalaqaItem? _selectedHalaqa;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private string _gender = "male";
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _residence = string.Empty;
    [ObservableProperty] private string? _maxStudents;
    [ObservableProperty] private string _timezone = "UTC";
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _nameError;
    [ObservableProperty] private string? _descriptionError;
    [ObservableProperty] private string? _genderError;
    [ObservableProperty] private string? _countryError;
    [ObservableProperty] private string? _residenceError;
    [ObservableProperty] private string? _maxStudentsError;
    [ObservableProperty] private string? _timezoneError;
    [ObservableProperty] private bool _isDialogOpen;

    public bool IsEditing => SelectedHalaqa is not null;
    public string EditorTitle => IsEditing ? "تعديل الحلقة المحددة" : "إنشاء حلقة جديدة";
    public string SaveTitle => IsEditing ? "حفظ التعديل" : "إنشاء الحلقة";
    public string ToggleStatusTitle => SelectedHalaqa?.Status == HalaqaStatus.Active ? "إيقاف الحلقة" : "تفعيل الحلقة";

    public event EventHandler? BackRequested;
    public event EventHandler<HalaqaItem>? MembershipsRequested;
    public event EventHandler<HalaqaItem>? RegistrationRequestsRequested;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadNextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            await LoadPageAsync(CurrentPage + 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            await LoadPageAsync(CurrentPage - 1);
        }
    }

    [RelayCommand]
    private void OpenCreateDialog()
    {
        ClearForm();
        ClearFeedback();
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditDialog(HalaqaItem? item)
    {
        if (item is not null)
        {
            SelectedHalaqa = item;
        }
        ClearFeedback();
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
        ClearFeedback();
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void NewHalaqa() => OpenCreateDialog();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!TryReadForm(out var values, out var error))
        {
            SetLocalFailure(error!);
            return;
        }

        var wasEditing = SelectedHalaqa is not null;
        IsBusy = true;
        ClearFeedback();
        try
        {
            Result<HalaqaItem> result;
            if (SelectedHalaqa is null)
            {
                result = await _createHalaqaUseCase.ExecuteAsync(new CreateHalaqaCommand(
                    values.Name,
                    values.Description,
                    values.Gender,
                    values.Country,
                    values.Residence,
                    values.MaxStudents,
                    values.Timezone,
                    HalaqaStatus.Active));
            }
            else
            {
                result = await _updateHalaqaUseCase.ExecuteAsync(new UpdateHalaqaCommand(
                    SelectedHalaqa.Id,
                    values.Name,
                    values.Description,
                    values.Gender,
                    values.Country,
                    values.Residence,
                    values.MaxStudents,
                    values.Timezone,
                    SelectedHalaqa.Status));
            }

            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            SelectedHalaqa = result.Value;
            Message = wasEditing ? "تم حفظ تعديلات الحلقة." : "تم إنشاء الحلقة.";
            IsDialogOpen = false;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanToggle))]
    private async Task ToggleStatusAsync()
    {
        var selected = SelectedHalaqa;
        if (selected is null)
        {
            return;
        }

        await ToggleStatusCoreAsync(selected);
    }

    [RelayCommand]
    private async Task ToggleStatusForItemAsync(HalaqaItem? item)
    {
        if (item is null)
        {
            return;
        }

        await ToggleStatusCoreAsync(item);
    }

    private async Task ToggleStatusCoreAsync(HalaqaItem halaqa)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = halaqa.Status == HalaqaStatus.Active
                ? await _deactivateHalaqaUseCase.ExecuteAsync(halaqa.Id)
                : await _activateHalaqaUseCase.ExecuteAsync(halaqa.Id);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            if (SelectedHalaqa?.Id == result.Value.Id)
            {
                SelectedHalaqa = result.Value;
            }
            Message = result.Value.Status == HalaqaStatus.Active ? "تم تفعيل الحلقة." : "تم إيقاف الحلقة مؤقتاً.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(CanOpenMemberships))]
    private void OpenMemberships()
    {
        if (SelectedHalaqa is { } halaqa)
        {
            MembershipsRequested?.Invoke(this, halaqa);
        }
    }

    [RelayCommand]
    private void OpenMembershipsForItem(HalaqaItem? item)
    {
        if (item is not null)
        {
            MembershipsRequested?.Invoke(this, item);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenRegistrationRequests))]
    private void OpenRegistrationRequests()
    {
        if (SelectedHalaqa is { } halaqa)
        {
            RegistrationRequestsRequested?.Invoke(this, halaqa);
        }
    }

    [RelayCommand]
    private void OpenRegistrationRequestsForItem(HalaqaItem? item)
    {
        if (item is not null)
        {
            RegistrationRequestsRequested?.Invoke(this, item);
        }
    }

    partial void OnSelectedHalaqaChanged(HalaqaItem? value)
    {
        if (value is null)
        {
            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(EditorTitle));
            OnPropertyChanged(nameof(SaveTitle));
            OnPropertyChanged(nameof(ToggleStatusTitle));
            ToggleStatusCommand.NotifyCanExecuteChanged();
            OpenMembershipsCommand.NotifyCanExecuteChanged();
            OpenRegistrationRequestsCommand.NotifyCanExecuteChanged();
            return;
        }

        Name = value.Name;
        Description = value.Description;
        Gender = ToContractValue(value.Gender);
        Country = value.Country;
        Residence = value.Residence;
        MaxStudents = value.MaxStudents?.ToString(CultureInfo.InvariantCulture);
        Timezone = value.Timezone;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(SaveTitle));
        OnPropertyChanged(nameof(ToggleStatusTitle));
        ToggleStatusCommand.NotifyCanExecuteChanged();
        OpenMembershipsCommand.NotifyCanExecuteChanged();
        OpenRegistrationRequestsCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnCountryChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnResidenceChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnTimezoneChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy;
    private bool CanEdit() => !IsBusy;
    private bool CanSave() => !IsBusy && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Country) && !string.IsNullOrWhiteSpace(Residence) && !string.IsNullOrWhiteSpace(Timezone);
    private bool CanToggle() => !IsBusy && SelectedHalaqa is not null;
    private bool CanOpenMemberships() => !IsBusy && SelectedHalaqa is not null;
    private bool CanOpenRegistrationRequests() => !IsBusy && SelectedHalaqa is not null;
    private bool CanNavigateBack() => !IsBusy;

    private async Task LoadPageAsync(int page)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listHalaqasUseCase.ExecuteAsync(page);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Halaqas.Clear();
            foreach (var halaqa in result.Value.Halaqas)
            {
                Halaqas.Add(halaqa);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            ClearForm();
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool TryReadForm(out FormValues values, out string? error)
    {
        values = default;
        error = null;
        if (!Enum.TryParse<HalaqaGender>(Gender, true, out var gender))
        {
            error = "اختر جنس الحلقة بصورة صحيحة.";
            return false;
        }
        int? maxStudents = null;
        if (!string.IsNullOrWhiteSpace(MaxStudents))
        {
            if (!int.TryParse(MaxStudents, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
            {
                error = "أدخل الحد الأقصى للطلاب كرقم موجب أو اتركه فارغاً.";
                return false;
            }
            maxStudents = parsed;
        }

        values = new FormValues(
            Name.Trim(),
            NormalizeOptional(Description),
            gender,
            Country.Trim(),
            Residence.Trim(),
            maxStudents,
            Timezone.Trim());
        return true;
    }

    private void Upsert(HalaqaItem halaqa)
    {
        var index = Halaqas.Select((value, index) => (value, index)).FirstOrDefault(item => item.value.Id == halaqa.Id).index;
        if (index >= 0 && index < Halaqas.Count && Halaqas[index].Id == halaqa.Id)
        {
            Halaqas[index] = halaqa;
        }
        else
        {
            Halaqas.Insert(0, halaqa);
            Total++;
        }
    }

    private void ClearForm()
    {
        SelectedHalaqa = null;
        Name = string.Empty;
        Description = null;
        Gender = "male";
        Country = string.Empty;
        Residence = string.Empty;
        MaxStudents = null;
        Timezone = "UTC";
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        NewHalaqaCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        ToggleStatusCommand.NotifyCanExecuteChanged();
        OpenMembershipsCommand.NotifyCanExecuteChanged();
        OpenRegistrationRequestsCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        NameError = null;
        DescriptionError = null;
        GenderError = null;
        CountryError = null;
        ResidenceError = null;
        MaxStudentsError = null;
        TimezoneError = null;
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
                    case "description": DescriptionError = fieldMessage; break;
                    case "gender": GenderError = fieldMessage; break;
                    case "country": CountryError = fieldMessage; break;
                    case "residence": ResidenceError = fieldMessage; break;
                    case "max_students": MaxStudentsError = fieldMessage; break;
                    case "timezone": TimezoneError = fieldMessage; break;
                }
            }
        }
        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private readonly record struct FormValues(
        string Name,
        string? Description,
        HalaqaGender Gender,
        string Country,
        string Residence,
        int? MaxStudents,
        string Timezone);
}
