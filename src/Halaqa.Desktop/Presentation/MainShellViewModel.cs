using CommunityToolkit.Mvvm.ComponentModel;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Presentation.ViewModels;

namespace Halaqa.Desktop.Presentation;

public sealed partial class MainShellViewModel : ObservableObject
{
    private readonly LoginViewModel _loginViewModel;
    private readonly StudentRegistrationViewModel _studentRegistrationViewModel;
    private readonly TeacherRegistrationViewModel _teacherRegistrationViewModel;
    private readonly ForgotPasswordViewModel _forgotPasswordViewModel;
    private readonly ResetPasswordViewModel _resetPasswordViewModel;
    private readonly GeneralProfileViewModel _generalProfileViewModel;
    private readonly RestoreSessionUseCase _restoreSessionUseCase;
    private AuthenticatedUser? _authenticatedUser;

    [ObservableProperty]
    private object? _currentPage;

    public MainShellViewModel(
        LoginViewModel loginViewModel,
        StudentRegistrationViewModel studentRegistrationViewModel,
        TeacherRegistrationViewModel teacherRegistrationViewModel,
        ForgotPasswordViewModel forgotPasswordViewModel,
        ResetPasswordViewModel resetPasswordViewModel,
        GeneralProfileViewModel generalProfileViewModel,
        RestoreSessionUseCase restoreSessionUseCase)
    {
        _loginViewModel = loginViewModel;
        _studentRegistrationViewModel = studentRegistrationViewModel;
        _teacherRegistrationViewModel = teacherRegistrationViewModel;
        _forgotPasswordViewModel = forgotPasswordViewModel;
        _resetPasswordViewModel = resetPasswordViewModel;
        _generalProfileViewModel = generalProfileViewModel;
        _restoreSessionUseCase = restoreSessionUseCase;

        _loginViewModel.SignedIn += (_, authenticatedUser) => ShowDashboard(authenticatedUser);
        _loginViewModel.StudentRegistrationRequested += (_, _) => CurrentPage = _studentRegistrationViewModel;
        _loginViewModel.TeacherRegistrationRequested += (_, _) => CurrentPage = _teacherRegistrationViewModel;
        _loginViewModel.PasswordRecoveryRequested += (_, _) => CurrentPage = _forgotPasswordViewModel;
        _forgotPasswordViewModel.ResetRequested += (_, email) =>
        {
            _resetPasswordViewModel.Email = email;
            CurrentPage = _resetPasswordViewModel;
        };
        _studentRegistrationViewModel.Registered += (_, authenticatedUser) => ShowDashboard(authenticatedUser);
        _teacherRegistrationViewModel.Registered += (_, authenticatedUser) => ShowDashboard(authenticatedUser);
        _generalProfileViewModel.BackRequested += (_, _) => ShowDashboard();
        _generalProfileViewModel.ProfileUpdated += (_, profile) => UpdateAuthenticatedUser(profile);

        CurrentPage = _loginViewModel;
    }

    public async Task RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var authenticatedUser = await _restoreSessionUseCase.ExecuteAsync(cancellationToken);
        if (authenticatedUser is not null)
        {
            ShowDashboard(authenticatedUser);
        }
    }

    private void ShowDashboard(AuthenticatedUser authenticatedUser)
    {
        _authenticatedUser = authenticatedUser;
        ShowDashboard();
    }

    private void ShowDashboard()
    {
        if (_authenticatedUser is null)
        {
            CurrentPage = _loginViewModel;
            return;
        }

        var dashboardViewModel = new DashboardViewModel(_authenticatedUser.User);
        dashboardViewModel.ProfileRequested += async (_, _) => await ShowProfileAsync();
        CurrentPage = dashboardViewModel;
    }

    private async Task ShowProfileAsync()
    {
        CurrentPage = _generalProfileViewModel;
        await _generalProfileViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void UpdateAuthenticatedUser(UserProfile profile)
    {
        if (_authenticatedUser is null)
        {
            return;
        }

        _authenticatedUser = _authenticatedUser with
        {
            User = new AuthUser(profile.Id, profile.Role, profile.Name, profile.Email, profile.Status)
        };
    }
}
