using CommunityToolkit.Mvvm.ComponentModel;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

namespace Halaqa.Desktop.Presentation;

public sealed partial class MainShellViewModel : ObservableObject
{
    private readonly LoginViewModel _loginViewModel;
    private readonly StudentRegistrationViewModel _studentRegistrationViewModel;
    private readonly TeacherRegistrationViewModel _teacherRegistrationViewModel;
    private readonly ForgotPasswordViewModel _forgotPasswordViewModel;

    [ObservableProperty]
    private object? _currentPage;

    public MainShellViewModel(
        LoginViewModel loginViewModel,
        StudentRegistrationViewModel studentRegistrationViewModel,
        TeacherRegistrationViewModel teacherRegistrationViewModel,
        ForgotPasswordViewModel forgotPasswordViewModel)
    {
        _loginViewModel = loginViewModel;
        _studentRegistrationViewModel = studentRegistrationViewModel;
        _teacherRegistrationViewModel = teacherRegistrationViewModel;
        _forgotPasswordViewModel = forgotPasswordViewModel;

        _loginViewModel.SignedIn += (_, authenticatedUser) => ShowDashboard(authenticatedUser);
        _loginViewModel.StudentRegistrationRequested += (_, _) => CurrentPage = _studentRegistrationViewModel;
        _loginViewModel.TeacherRegistrationRequested += (_, _) => CurrentPage = _teacherRegistrationViewModel;
        _loginViewModel.PasswordRecoveryRequested += (_, _) => CurrentPage = _forgotPasswordViewModel;
        _studentRegistrationViewModel.Registered += (_, authenticatedUser) => ShowDashboard(authenticatedUser);
        _teacherRegistrationViewModel.Registered += (_, authenticatedUser) => ShowDashboard(authenticatedUser);

        CurrentPage = _loginViewModel;
    }

    private void ShowDashboard(AuthenticatedUser authenticatedUser) =>
        CurrentPage = new DashboardViewModel(authenticatedUser.User);
}
