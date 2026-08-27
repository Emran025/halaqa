using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class ChangePasswordViewModel : ObservableObject
{
    private readonly ChangePasswordUseCase changePasswordUseCase;

    public ChangePasswordViewModel(ChangePasswordUseCase changePasswordUseCase)
    {
        this.changePasswordUseCase = changePasswordUseCase;
    }

    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirmation = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _currentPasswordError;
    [ObservableProperty] private string? _passwordError;
    [ObservableProperty] private string? _passwordConfirmationError;

    public event EventHandler? BackRequested;
    public event EventHandler? PasswordChanged;
    public event EventHandler? SensitiveInputsCleared;

    public void Initialize()
    {
        ClearSensitiveInputs();
        ClearFeedback();
        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        IsBusy = true;
        ClearFeedback();
        SubmitCommand.NotifyCanExecuteChanged();
        try
        {
            var result = await changePasswordUseCase.ExecuteAsync(CurrentPassword, Password, PasswordConfirmation);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            ClearSensitiveInputs();
            Message = "تم تغيير كلمة المرور بنجاح.";
            PasswordChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
            SubmitCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanSubmit() => !IsBusy && !string.IsNullOrWhiteSpace(CurrentPassword) &&
                                !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(PasswordConfirmation);

    private bool CanNavigateBack() => !IsBusy;

    private void ClearSensitiveInputs()
    {
        CurrentPassword = string.Empty;
        Password = string.Empty;
        PasswordConfirmation = string.Empty;
        SensitiveInputsCleared?.Invoke(this, EventArgs.Empty);
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        CurrentPasswordError = null;
        PasswordError = null;
        PasswordConfirmationError = null;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تغيير كلمة المرور. أعد المحاولة.";
        if (error?.FieldErrors is null)
        {
            return;
        }

        foreach (var fieldError in error.FieldErrors)
        {
            var fieldMessage = string.Join(" ", fieldError.Messages);
            switch (fieldError.Field)
            {
                case "current_password":
                    CurrentPasswordError = fieldMessage;
                    break;
                case "password":
                    PasswordError = fieldMessage;
                    break;
                case "password_confirmation":
                    PasswordConfirmationError = fieldMessage;
                    break;
            }
        }
    }

    partial void OnCurrentPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordConfirmationChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => BackCommand.NotifyCanExecuteChanged();
}
