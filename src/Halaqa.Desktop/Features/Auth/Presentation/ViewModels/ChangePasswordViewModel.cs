using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class ChangePasswordViewModel : ObservableObject
{

    private readonly ChangePasswordUseCase changePasswordUseCase;


    public ChangePasswordViewModel(

        ChangePasswordUseCase changePasswordUseCase

    )

    {

        this.changePasswordUseCase = changePasswordUseCase;

    }

    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirmation = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        IsBusy = true;
        IsError = false;
        Message = null;
        SubmitCommand.NotifyCanExecuteChanged();
        try
        {
            var result = await changePasswordUseCase.ExecuteAsync(CurrentPassword, Password, PasswordConfirmation);
            IsError = !result.IsSuccess;
            Message = result.IsSuccess ? "تم تغيير كلمة المرور بنجاح." : result.Error?.Message;
            if (result.IsSuccess)
            {
                CurrentPassword = string.Empty;
                Password = string.Empty;
                PasswordConfirmation = string.Empty;
            }
        }
        finally
        {
            IsBusy = false;
            SubmitCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanSubmit() => !IsBusy && !string.IsNullOrWhiteSpace(CurrentPassword) &&
                                !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(PasswordConfirmation);

    partial void OnCurrentPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordConfirmationChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
}
