using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Auth;

public sealed class ChangePasswordViewModelTests
{
    [Fact]
    public async Task Submit_SendsContractFieldsAndClearsSensitiveInputsAfterSuccess()
    {
        var repository = new FakeAuthRepository();
        var viewModel = new ChangePasswordViewModel(new ChangePasswordUseCase(repository))
        {
            CurrentPassword = "current-password",
            Password = "new-password",
            PasswordConfirmation = "new-password"
        };
        var changed = false;
        viewModel.PasswordChanged += (_, _) => changed = true;

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(("current-password", "new-password", "new-password"), repository.ChangeRequest);
        Assert.True(changed);
        Assert.False(viewModel.IsError);
        Assert.Empty(viewModel.CurrentPassword);
        Assert.Empty(viewModel.Password);
        Assert.Empty(viewModel.PasswordConfirmation);
        Assert.Equal("تم تغيير كلمة المرور بنجاح.", viewModel.Message);
    }

    [Fact]
    public async Task Submit_MapsContractFieldErrorsWithoutClearingInputs()
    {
        var repository = new FakeAuthRepository
        {
            ChangeResult = Result.Failure(AppError.Validation("تعذر تغيير كلمة المرور.", new[]
            {
                new FieldError("current_password", new[] { "كلمة المرور الحالية غير صحيحة." }),
                new FieldError("password", new[] { "كلمة المرور يجب أن تتكون من ثمانية أحرف على الأقل." }),
                new FieldError("password_confirmation", new[] { "تأكيد كلمة المرور غير مطابق." })
            }))
        };
        var viewModel = new ChangePasswordViewModel(new ChangePasswordUseCase(repository))
        {
            CurrentPassword = "current-password",
            Password = "new-password",
            PasswordConfirmation = "new-password"
        };

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsError);
        Assert.Equal("تعذر تغيير كلمة المرور.", viewModel.Message);
        Assert.Equal("كلمة المرور الحالية غير صحيحة.", viewModel.CurrentPasswordError);
        Assert.Equal("كلمة المرور يجب أن تتكون من ثمانية أحرف على الأقل.", viewModel.PasswordError);
        Assert.Equal("تأكيد كلمة المرور غير مطابق.", viewModel.PasswordConfirmationError);
        Assert.Equal("current-password", viewModel.CurrentPassword);
        Assert.Equal("new-password", viewModel.Password);
        Assert.Equal("new-password", viewModel.PasswordConfirmation);
    }

    [Fact]
    public void Back_RaisesBackRequestedWhenNotBusy()
    {
        var viewModel = new ChangePasswordViewModel(new ChangePasswordUseCase(new FakeAuthRepository()));
        var raised = false;
        viewModel.BackRequested += (_, _) => raised = true;

        viewModel.BackCommand.Execute(null);

        Assert.True(raised);
    }

    private sealed class FakeAuthRepository : IAuthRepository
    {
        public (string CurrentPassword, string Password, string PasswordConfirmation)? ChangeRequest { get; private set; }
        public Result ChangeResult { get; set; } = Result.Success();

        public Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterStudentAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterTeacherAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ResetPasswordAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Result> ChangePasswordAsync(string currentPassword, string password, string passwordConfirmation, CancellationToken cancellationToken = default)
        {
            ChangeRequest = (currentPassword, password, passwordConfirmation);
            return Task.FromResult(ChangeResult);
        }

        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
