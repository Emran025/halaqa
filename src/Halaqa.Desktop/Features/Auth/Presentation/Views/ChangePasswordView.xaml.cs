using System.Windows;
using System.Windows.Controls;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

namespace Halaqa.Desktop.Features.Auth.Presentation.Views;

public partial class ChangePasswordView : UserControl
{
    public ChangePasswordView()
    {
        InitializeComponent();
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ChangePasswordViewModel viewModel)
        {
            return;
        }

        viewModel.CurrentPassword = CurrentPasswordInput.Password;
        viewModel.Password = PasswordInput.Password;
        viewModel.PasswordConfirmation = PasswordConfirmationInput.Password;
    }
}
