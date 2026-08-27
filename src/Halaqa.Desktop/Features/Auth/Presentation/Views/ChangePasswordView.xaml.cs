using System.Windows;
using System.Windows.Controls;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

namespace Halaqa.Desktop.Features.Auth.Presentation.Views;

public partial class ChangePasswordView : UserControl
{
    private ChangePasswordViewModel? viewModel;

    public ChangePasswordView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (viewModel is not null)
        {
            viewModel.SensitiveInputsCleared -= OnSensitiveInputsCleared;
        }

        viewModel = e.NewValue as ChangePasswordViewModel;
        if (viewModel is not null)
        {
            viewModel.SensitiveInputsCleared += OnSensitiveInputsCleared;
        }

        ClearPasswordInputs();
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ChangePasswordViewModel currentViewModel)
        {
            return;
        }

        currentViewModel.CurrentPassword = CurrentPasswordInput.Password;
        currentViewModel.Password = PasswordInput.Password;
        currentViewModel.PasswordConfirmation = PasswordConfirmationInput.Password;
    }

    private void OnSensitiveInputsCleared(object? sender, EventArgs e) => ClearPasswordInputs();

    private void ClearPasswordInputs()
    {
        CurrentPasswordInput?.Clear();
        PasswordInput?.Clear();
        PasswordConfirmationInput?.Clear();
    }
}
