using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class ResetPasswordViewModel : ObservableObject
{

    private readonly ResetPasswordUseCase resetPasswordUseCase;


    public ResetPasswordViewModel(

        ResetPasswordUseCase resetPasswordUseCase

    )

    {

        this.resetPasswordUseCase = resetPasswordUseCase;

    }

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _token = string.Empty;
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
            var result = await resetPasswordUseCase.ExecuteAsync(Email, Token, Password, PasswordConfirmation);
            IsError = !result.IsSuccess;
            Message = result.IsSuccess
                ? "تم تغيير كلمة المرور. يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة."
                : result.Error?.Message;
            if (result.IsSuccess)
            {
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

    private bool CanSubmit() => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Token) &&
                                !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(PasswordConfirmation);

    partial void OnEmailChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnTokenChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordConfirmationChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
}
