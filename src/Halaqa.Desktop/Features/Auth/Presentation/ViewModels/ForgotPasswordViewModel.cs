using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;

namespace Halaqa.Desktop.Features.Auth.Presentation.ViewModels;

public sealed partial class ForgotPasswordViewModel(RequestPasswordResetUseCase requestPasswordResetUseCase) : ObservableObject
{
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isError;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        IsBusy = true;
        Message = null;
        IsError = false;
        SubmitCommand.NotifyCanExecuteChanged();
        try
        {
            var result = await requestPasswordResetUseCase.ExecuteAsync(Email);
            IsError = !result.IsSuccess;
            Message = result.IsSuccess
                ? "إذا كان البريد مسجلاً، ستصل إليه تعليمات إعادة التعيين."
                : result.Error?.Message;
        }
        finally
        {
            IsBusy = false;
            SubmitCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanSubmit() => !IsBusy && !string.IsNullOrWhiteSpace(Email);
    partial void OnEmailChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
}
