using System.Windows;
using System.Windows.Controls;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

namespace Halaqa.Desktop.Features.Auth.Presentation.Views;

public partial class TeacherRegistrationView : UserControl
{
    public TeacherRegistrationView()
    {
        InitializeComponent();
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not TeacherRegistrationViewModel viewModel)
        {
            return;
        }

        viewModel.Password = PasswordInput.Password;
        viewModel.PasswordConfirmation = PasswordConfirmationInput.Password;
    }
}
