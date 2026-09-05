using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{

    private readonly LoginUseCase loginUseCase;
    private readonly ResendVerificationUseCase resendVerificationUseCase;


    public LoginViewModel(

        LoginUseCase loginUseCase,
        ResendVerificationUseCase resendVerificationUseCase

    )

    {

        this.loginUseCase = loginUseCase;
        this.resendVerificationUseCase = resendVerificationUseCase;

    }

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

    public event EventHandler<AuthenticatedUser>? SignedIn;
    public event EventHandler? StudentRegistrationRequested;
    public event EventHandler? TeacherRegistrationRequested;
    public event EventHandler? PasswordRecoveryRequested;

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
            SignedIn?.Invoke(this, result.Value);
        }
        finally
        {
            IsBusy = false;
            LoginCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void OpenStudentRegistration() => StudentRegistrationRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenTeacherRegistration() => TeacherRegistrationRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenPasswordRecovery() => PasswordRecoveryRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(CanResendVerification))]
    private async Task ResendVerificationAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;
        IsBusy = true;
        ResendVerificationCommand.NotifyCanExecuteChanged();
        try
        {
            var result = await resendVerificationUseCase.ExecuteAsync(Email);
            StatusMessage = result.IsSuccess
                ? "إذا كان الحساب غير مفعّل، فستصلك رسالة تفعيل جديدة. افحص البريد والرسائل غير المرغوب فيها."
                : result.Error?.Message ?? "تعذر إرسال رسالة التفعيل.";
        }
        finally
        {
            IsBusy = false;
            ResendVerificationCommand.NotifyCanExecuteChanged();
        }
    }

    public void ShowVerificationPending(string email)
    {
        Email = email;
        Password = string.Empty;
        ErrorMessage = null;
        StatusMessage = "تم إنشاء الحساب. افتح رسالة التفعيل في بريدك ثم عد إلى التطبيق لتسجيل الدخول.";
    }

    private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    private bool CanResendVerification() => !IsBusy && !string.IsNullOrWhiteSpace(Email);

    partial void OnEmailChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
        ResendVerificationCommand.NotifyCanExecuteChanged();
    }
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
