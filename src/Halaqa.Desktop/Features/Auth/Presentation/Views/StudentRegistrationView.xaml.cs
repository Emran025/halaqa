using System.Windows;
using System.Windows.Controls;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

namespace Halaqa.Desktop.Features.Auth.Presentation.Views;

public partial class StudentRegistrationView : UserControl
{
    public StudentRegistrationView()
    {
        InitializeComponent();
    }

    private void PasswordChanged(object sender, RoutedEventArgs eventArgs)
    {
        if (DataContext is not StudentRegistrationViewModel viewModel || sender is not PasswordBox passwordBox)
        {
            return;
        }

        if (passwordBox.Name == "PasswordInput")
        {
            viewModel.Password = passwordBox.Password;
        }
        else if (passwordBox.Name == "PasswordConfirmationInput")
        {
            viewModel.PasswordConfirmation = passwordBox.Password;
        }
    }
}
