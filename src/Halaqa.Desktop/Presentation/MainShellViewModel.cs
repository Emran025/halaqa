using CommunityToolkit.Mvvm.ComponentModel;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;
using Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;
using Halaqa.Desktop.Features.Halaqas.Presentation.ViewModels;
using Halaqa.Desktop.Features.Memberships.Presentation.ViewModels;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Presentation.ViewModels;
using Halaqa.Desktop.Features.Profile.Presentation.ViewModels;
using Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;
using Halaqa.Desktop.Features.TeacherDocuments.Presentation.ViewModels;

namespace Halaqa.Desktop.Presentation;

public sealed partial class MainShellViewModel : ObservableObject
{
    private readonly LoginViewModel _loginViewModel;
    private readonly StudentRegistrationViewModel _studentRegistrationViewModel;
    private readonly TeacherRegistrationViewModel _teacherRegistrationViewModel;
    private readonly ForgotPasswordViewModel _forgotPasswordViewModel;
    private readonly ResetPasswordViewModel _resetPasswordViewModel;
    private readonly GeneralProfileViewModel _generalProfileViewModel;
    private readonly StudentProfileViewModel _studentProfileViewModel;
    private readonly TeacherProfileViewModel _teacherProfileViewModel;
    private readonly TeacherDocumentsViewModel _teacherDocumentsViewModel;
    private readonly HalaqasViewModel _halaqasViewModel;
    private readonly HalaqaMembershipsViewModel _halaqaMembershipsViewModel;
    private readonly HalaqaRegistrationRequestsViewModel _halaqaRegistrationRequestsViewModel;
    private readonly StudentTeacherDirectoryViewModel _studentTeacherDirectoryViewModel;
    private readonly StudentRegistrationRequestsViewModel _studentRegistrationRequestsViewModel;
    private readonly FollowUpViewModel _followUpViewModel;
    private readonly QuranReaderViewModel _quranReaderViewModel;
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
        StudentProfileViewModel studentProfileViewModel,
        TeacherProfileViewModel teacherProfileViewModel,
        TeacherDocumentsViewModel teacherDocumentsViewModel,
        HalaqasViewModel halaqasViewModel,
        HalaqaMembershipsViewModel halaqaMembershipsViewModel,
        HalaqaRegistrationRequestsViewModel halaqaRegistrationRequestsViewModel,
        StudentTeacherDirectoryViewModel studentTeacherDirectoryViewModel,
        StudentRegistrationRequestsViewModel studentRegistrationRequestsViewModel,
        FollowUpViewModel followUpViewModel,
        QuranReaderViewModel quranReaderViewModel,
        RestoreSessionUseCase restoreSessionUseCase)
    {
        _loginViewModel = loginViewModel;
        _studentRegistrationViewModel = studentRegistrationViewModel;
        _teacherRegistrationViewModel = teacherRegistrationViewModel;
        _forgotPasswordViewModel = forgotPasswordViewModel;
        _resetPasswordViewModel = resetPasswordViewModel;
        _generalProfileViewModel = generalProfileViewModel;
        _studentProfileViewModel = studentProfileViewModel;
        _teacherProfileViewModel = teacherProfileViewModel;
        _teacherDocumentsViewModel = teacherDocumentsViewModel;
        _halaqasViewModel = halaqasViewModel;
        _halaqaMembershipsViewModel = halaqaMembershipsViewModel;
        _halaqaRegistrationRequestsViewModel = halaqaRegistrationRequestsViewModel;
        _studentTeacherDirectoryViewModel = studentTeacherDirectoryViewModel;
        _studentRegistrationRequestsViewModel = studentRegistrationRequestsViewModel;
        _followUpViewModel = followUpViewModel;
        _quranReaderViewModel = quranReaderViewModel;
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
        _studentProfileViewModel.BackRequested += (_, _) => ShowDashboard();
        _studentProfileViewModel.ProfileUpdated += (_, profile) => UpdateAuthenticatedUser(profile);
        _teacherProfileViewModel.BackRequested += (_, _) => ShowDashboard();
        _teacherProfileViewModel.DocumentsRequested += async (_, _) => await ShowTeacherDocumentsAsync();
        _teacherProfileViewModel.ProfileUpdated += (_, profile) => UpdateAuthenticatedUser(profile);
        _teacherDocumentsViewModel.BackRequested += (_, _) => CurrentPage = _teacherProfileViewModel;
        _halaqasViewModel.BackRequested += (_, _) => ShowDashboard();
        _halaqasViewModel.MembershipsRequested += async (_, halaqa) => await ShowHalaqaMembershipsAsync(halaqa.Id, halaqa.Name);
        _halaqasViewModel.RegistrationRequestsRequested += async (_, halaqa) => await ShowHalaqaRegistrationRequestsAsync(halaqa.Id, halaqa.Name);
        _halaqaMembershipsViewModel.BackRequested += (_, _) => CurrentPage = _halaqasViewModel;
        _halaqaRegistrationRequestsViewModel.BackRequested += (_, _) => CurrentPage = _halaqasViewModel;
        _studentTeacherDirectoryViewModel.BackRequested += (_, _) => ShowDashboard();
        _studentTeacherDirectoryViewModel.MyRequestsRequested += async (_, _) => await ShowStudentRegistrationRequestsAsync();
        _studentRegistrationRequestsViewModel.BackRequested += (_, _) => CurrentPage = _studentTeacherDirectoryViewModel;
        _followUpViewModel.BackRequested += (_, _) => ShowDashboard();
        _quranReaderViewModel.BackRequested += (_, _) => ShowDashboard();

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
        dashboardViewModel.StudentProfileRequested += async (_, _) => await ShowStudentProfileAsync();
        dashboardViewModel.TeacherProfileRequested += async (_, _) => await ShowTeacherProfileAsync();
        dashboardViewModel.HalaqasRequested += async (_, _) => await ShowHalaqasAsync();
        dashboardViewModel.StudentRegistrationsRequested += async (_, _) => await ShowStudentTeacherDirectoryAsync();
        dashboardViewModel.FollowUpRequested += async (_, _) => await ShowFollowUpAsync();
        dashboardViewModel.QuranReaderRequested += async (_, _) => await ShowQuranReaderAsync();
        CurrentPage = dashboardViewModel;
    }

    private async Task ShowProfileAsync()
    {
        CurrentPage = _generalProfileViewModel;
        await _generalProfileViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowStudentProfileAsync()
    {
        CurrentPage = _studentProfileViewModel;
        await _studentProfileViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowTeacherProfileAsync()
    {
        CurrentPage = _teacherProfileViewModel;
        await _teacherProfileViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowTeacherDocumentsAsync()
    {
        CurrentPage = _teacherDocumentsViewModel;
        await _teacherDocumentsViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowHalaqasAsync()
    {
        CurrentPage = _halaqasViewModel;
        await _halaqasViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowQuranReaderAsync()
    {
        _quranReaderViewModel.Initialize();
        CurrentPage = _quranReaderViewModel;
        await _quranReaderViewModel.LoadPageCommand.ExecuteAsync(null);
    }

    private async Task ShowFollowUpAsync()
    {
        if (_authenticatedUser?.User.Role != UserRole.Student)
        {
            return;
        }

        _followUpViewModel.Initialize(_authenticatedUser.User.Id);
        CurrentPage = _followUpViewModel;
        await _followUpViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowStudentTeacherDirectoryAsync()
    {
        _studentTeacherDirectoryViewModel.Initialize();
        CurrentPage = _studentTeacherDirectoryViewModel;
        await _studentTeacherDirectoryViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowStudentRegistrationRequestsAsync()
    {
        _studentRegistrationRequestsViewModel.Initialize();
        CurrentPage = _studentRegistrationRequestsViewModel;
        await _studentRegistrationRequestsViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowHalaqaMembershipsAsync(Guid halaqaId, string halaqaName)
    {
        _halaqaMembershipsViewModel.Initialize(halaqaId, halaqaName);
        CurrentPage = _halaqaMembershipsViewModel;
        await _halaqaMembershipsViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async Task ShowHalaqaRegistrationRequestsAsync(Guid halaqaId, string halaqaName)
    {
        _halaqaRegistrationRequestsViewModel.Initialize(halaqaId, halaqaName);
        CurrentPage = _halaqaRegistrationRequestsViewModel;
        await _halaqaRegistrationRequestsViewModel.LoadCommand.ExecuteAsync(null);
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

    private void UpdateAuthenticatedUser(StudentProfile profile)
    {
        if (_authenticatedUser is null)
        {
            return;
        }

        _authenticatedUser = _authenticatedUser with
        {
            User = new AuthUser(profile.Id, UserRole.Student, profile.Name, profile.Email, profile.Status)
        };
    }

    private void UpdateAuthenticatedUser(TeacherProfile profile)
    {
        if (_authenticatedUser is null)
        {
            return;
        }

        _authenticatedUser = _authenticatedUser with
        {
            User = new AuthUser(profile.Id, UserRole.Teacher, profile.DisplayName, profile.Email ?? _authenticatedUser.User.Email, _authenticatedUser.User.Status)
        };
    }
}
