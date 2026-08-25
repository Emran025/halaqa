using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Profile.Domain;

public sealed class UpdateCurrentProfileUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_RejectsCommandWithoutChanges()
    {
        var repository = new FakeProfileRepository();
        var command = new UpdateUserProfileCommand(
            ProfileUpdateField<string>.Omit(),
            ProfileUpdateField<string>.Omit(),
            ProfileUpdateField<string>.Omit(),
            ProfileUpdateField<string>.Omit());

        var result = await new UpdateCurrentProfileUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.LastUpdate);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsTooShortNameBeforeCallingRepository()
    {
        var repository = new FakeProfileRepository();
        var command = new UpdateUserProfileCommand(
            ProfileUpdateField<string>.Set("أ"),
            ProfileUpdateField<string>.Omit(),
            ProfileUpdateField<string>.Omit(),
            ProfileUpdateField<string>.Omit());

        var result = await new UpdateCurrentProfileUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.LastUpdate);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsValidPartialUpdateIncludingExplicitNull()
    {
        var repository = new FakeProfileRepository();
        var command = new UpdateUserProfileCommand(
            ProfileUpdateField<string>.Set("طالب اختبار"),
            ProfileUpdateField<string>.Set(null),
            ProfileUpdateField<string>.Set("خمسة أجزاء"),
            ProfileUpdateField<string>.Omit());

        var result = await new UpdateCurrentProfileUseCase(repository).ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Same(command, repository.LastUpdate);
        Assert.True(repository.LastUpdate?.Phone.IsSpecified);
        Assert.Null(repository.LastUpdate?.Phone.Value);
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public UpdateUserProfileCommand? LastUpdate { get; private set; }

        public Task<Result<UserProfile>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<UserProfile>.Success(CreateProfile()));

        public Task<Result<UserProfile>> UpdateCurrentAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdate = command;
            return Task.FromResult(Result<UserProfile>.Success(CreateProfile()));
        }

        private static UserProfile CreateProfile() => new(
            Guid.NewGuid(),
            UserRole.Student,
            "طالب اختبار",
            "student@example.test",
            null,
            "active");
    }
}
