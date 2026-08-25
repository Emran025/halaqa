using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class LoginViewModel(LoginUseCase loginUseCase) : ObservableObject
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasStatus => !string.IsNullOrWhiteSpace(StatusMessage);

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));
    partial void OnStatusMessageChanged(string? value) => OnPropertyChanged(nameof(HasStatus));

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;
        IsBusy = true;
        LoginCommand.NotifyCanExecuteChanged();

        try
        {
            var result = await loginUseCase.ExecuteAsync(Email, Password);
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = RenderError(result.Error);
                return;
            }

            StatusMessage = $"تم تسجيل الدخول بنجاح. مرحباً {result.Value.User.Name}.";
            Password = string.Empty;
        }
        finally
        {
            IsBusy = false;
            LoginCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

    partial void OnEmailChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();

    private static string RenderError(AppError? error)
    {
        if (error is null)
        {
            return "تعذر إتمام تسجيل الدخول.";
        }

        if (error.Kind == AppErrorKind.Validation && error.FieldErrors?.Count > 0)
        {
            return string.Join(Environment.NewLine, error.FieldErrors.SelectMany(item => item.Messages));
        }

        return error.Message;
    }
}
