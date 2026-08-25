using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Auth;

public sealed class RegisterTeacherUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ForwardsValidRegistrationWithItsOperationId()
    {
        var repository = new FakeAuthRepository();
        var operationId = Guid.NewGuid();
        var command = new TeacherRegistrationCommand(
            operationId, "معلم اختبار", null, "teacher@example.test", "password8", "password8", Gender.Male,
            new DateOnly(1990, 1, 1), "Saudi Arabia", "Riyadh", null, "500000000", "+966", null, null,
            "إجازة في رواية حفص", 8, null, null, 3);

        var result = await new RegisterTeacherUseCase(repository).ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Same(command, repository.LastTeacherRegistration);
        Assert.Equal(operationId, repository.LastTeacherRegistration?.ClientOperationId);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsTeacherWithoutQualification()
    {
        var repository = new FakeAuthRepository();
        var command = new TeacherRegistrationCommand(
            Guid.NewGuid(), "معلم اختبار", null, "teacher@example.test", "password8", "password8", Gender.Male,
            new DateOnly(1990, 1, 1), "Saudi Arabia", "Riyadh", null, "500000000", "+966", null, null,
            string.Empty, 8, null, null, null);

        var result = await new RegisterTeacherUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.LastTeacherRegistration);
    }

    private sealed class FakeAuthRepository : IAuthRepository
    {
        public TeacherRegistrationCommand? LastTeacherRegistration { get; private set; }

        public Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterStudentAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterTeacherAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default)
        {
            LastTeacherRegistration = command;
            return Task.FromResult(Result<AuthenticatedUser>.Success(new AuthenticatedUser(
                new AuthUser(Guid.NewGuid(), UserRole.Teacher, "معلم اختبار", "teacher@example.test", "active"),
                "token", DateTimeOffset.UtcNow.AddHours(1))));
        }
        public Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ResetPasswordAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ChangePasswordAsync(string currentPassword, string password, string passwordConfirmation, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
