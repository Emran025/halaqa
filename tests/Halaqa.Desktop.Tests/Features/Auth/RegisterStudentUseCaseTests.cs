using Xunit;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Tests.Features.Auth;

public sealed class RegisterStudentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_RejectsRegistrationWithoutAvailabilityOrPlanDetails()
    {
        var repository = new FakeAuthRepository();
        var command = new StudentRegistrationCommand(
            Guid.NewGuid(), "طالب اختبار", null, "student@example.test", "password8", "password8", Gender.Male,
            new DateOnly(2010, 1, 1), "Saudi Arabia", "Riyadh", null, "500000000", "+966", null, null,
            null, null,
            new AttendancePreferences("Asia/Riyadh", Array.Empty<WeeklyAvailabilitySlot>(), 30),
            new FollowUpPlan(FollowUpFrequency.Daily, Array.Empty<FollowUpPlanDetail>(), null, null),
            null, null);

        var result = await new RegisterStudentUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.False(repository.StudentRegistrationCalled);
    }

    private sealed class FakeAuthRepository : IAuthRepository
    {
        public bool StudentRegistrationCalled { get; private set; }
        public Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AuthenticatedUser>> RegisterStudentAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default)
        {
            StudentRegistrationCalled = true;
            return Task.FromResult(Result<AuthenticatedUser>.Failure(new AppError(AppErrorKind.Unknown, "لا يجب استدعاؤه.")));
        }
        public Task<Result<AuthenticatedUser>> RegisterTeacherAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ResetPasswordAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ChangePasswordAsync(string currentPassword, string password, string passwordConfirmation, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
