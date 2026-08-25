using CommunityToolkit.Mvvm.ComponentModel;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

namespace Halaqa.Desktop.Presentation;

public sealed partial class MainShellViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentPage;

    public MainShellViewModel(LoginViewModel loginViewModel)
    {
        loginViewModel.SignedIn += (_, authenticatedUser) =>
            CurrentPage = new DashboardViewModel(authenticatedUser.User);
        CurrentPage = loginViewModel;
    }
}
