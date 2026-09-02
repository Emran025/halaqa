using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Presentation.ViewModels;

public sealed partial class GeneralProfileViewModel : ObservableObject
{

    private readonly GetCurrentProfileUseCase getCurrentProfileUseCase;

    private readonly UpdateCurrentProfileUseCase updateCurrentProfileUseCase;


    public GeneralProfileViewModel(

        GetCurrentProfileUseCase getCurrentProfileUseCase,

        UpdateCurrentProfileUseCase updateCurrentProfileUseCase

    )

    {

        this.getCurrentProfileUseCase = getCurrentProfileUseCase;

        this.updateCurrentProfileUseCase = updateCurrentProfileUseCase;

    }

    private string _loadedName = string.Empty;
    private string? _loadedPhone;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _memorizationLevel;
    [ObservableProperty] private string? _reviewLevel;
    [ObservableProperty] private bool _clearMemorizationLevel;
    [ObservableProperty] private bool _clearReviewLevel;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _nameError;
    [ObservableProperty] private string? _phoneError;
    [ObservableProperty] private string? _memorizationLevelError;
    [ObservableProperty] private string? _reviewLevelError;

    public event EventHandler? BackRequested;
    public event EventHandler<UserProfile>? ProfileUpdated;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        IsBusy = true;
        NotifyCommandCanExecuteChanged();
        ClearFeedback();

        try
        {
            var result = await getCurrentProfileUseCase.ExecuteAsync();
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Name = result.Value.Name;
            Email = result.Value.Email;
            Phone = result.Value.Phone;
            _loadedName = result.Value.Name;
            _loadedPhone = result.Value.Phone;
            MemorizationLevel = null;
            ReviewLevel = null;
            ClearMemorizationLevel = false;
            ClearReviewLevel = false;
        }
        finally
        {
            IsBusy = false;
            NotifyCommandCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsBusy = true;
        NotifyCommandCanExecuteChanged();
        ClearFeedback();

        try
        {
            var result = await updateCurrentProfileUseCase.ExecuteAsync(CreateUpdateCommand());
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Name = result.Value.Name;
            Email = result.Value.Email;
            Phone = result.Value.Phone;
            _loadedName = result.Value.Name;
            _loadedPhone = result.Value.Phone;
            MemorizationLevel = null;
            ReviewLevel = null;
            ClearMemorizationLevel = false;
            ClearReviewLevel = false;
            Message = "تم حفظ الملف الشخصي.";
            ProfileUpdated?.Invoke(this, result.Value);
        }
        finally
        {
            IsBusy = false;
            NotifyCommandCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPhoneChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnMemorizationLevelChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnReviewLevelChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnClearMemorizationLevelChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnClearReviewLevelChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy;

    private bool CanSave() => !IsBusy && CreateUpdateCommand().HasChanges;

    private bool CanNavigateBack() => !IsBusy;

    private UpdateUserProfileCommand CreateUpdateCommand() => new(
        !string.Equals(Name, _loadedName, StringComparison.Ordinal)
            ? ProfileUpdateField<string>.Set(Name.Trim())
            : ProfileUpdateField<string>.Omit(),
        !string.Equals(Phone, _loadedPhone, StringComparison.Ordinal)
            ? ProfileUpdateField<string>.Set(string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim())
            : ProfileUpdateField<string>.Omit(),
        ClearMemorizationLevel
            ? ProfileUpdateField<string>.Set(null)
            : !string.IsNullOrWhiteSpace(MemorizationLevel)
                ? ProfileUpdateField<string>.Set(MemorizationLevel.Trim())
                : ProfileUpdateField<string>.Omit(),
        ClearReviewLevel
            ? ProfileUpdateField<string>.Set(null)
            : !string.IsNullOrWhiteSpace(ReviewLevel)
                ? ProfileUpdateField<string>.Set(ReviewLevel.Trim())
                : ProfileUpdateField<string>.Omit());

    private void NotifyCommandCanExecuteChanged()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        NameError = null;
        PhoneError = null;
        MemorizationLevelError = null;
        ReviewLevelError = null;
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
                    case "name":
                        NameError = fieldMessage;
                        break;
                    case "phone":
                        PhoneError = fieldMessage;
                        break;
                    case "memorization_level":
                        MemorizationLevelError = fieldMessage;
                        break;
                    case "review_level":
                        ReviewLevelError = fieldMessage;
                        break;
                }
            }
        }

        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }
}
