using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Auth;

public sealed class PasswordUseCaseTests
{
    [Fact]
    public async Task ResetPassword_ForwardsCompleteContractFields()
    {
        var repository = new FakeAuthRepository();

        var result = await new ResetPasswordUseCase(repository).ExecuteAsync(
            "user@example.test", "reset-token", "password8", "password8");

        Assert.True(result.IsSuccess);
        Assert.Equal(("user@example.test", "reset-token", "password8", "password8"), repository.ResetRequest);
    }

    [Fact]
    public async Task ChangePassword_RejectsMismatchedConfirmationBeforeRepository()
    {
        var repository = new FakeAuthRepository();

        var result = await new ChangePasswordUseCase(repository).ExecuteAsync("current8", "password8", "different8");

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.False(repository.ChangeCalled);
    }

    private sealed class FakeAuthRepository : IAuthRepository
    {
        public (string Email, string Token, string Password, string PasswordConfirmation)? ResetRequest { get; private set; }
        public bool ChangeCalled { get; private set; }

        public Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterStudentAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterTeacherAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ResetPasswordAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken = default)
        {
            ResetRequest = (email, token, password, passwordConfirmation);
            return Task.FromResult(Result.Success());
        }
        public Task<Result> ChangePasswordAsync(string currentPassword, string password, string passwordConfirmation, CancellationToken cancellationToken = default)
        {
            ChangeCalled = true;
            return Task.FromResult(Result.Success());
        }
        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
