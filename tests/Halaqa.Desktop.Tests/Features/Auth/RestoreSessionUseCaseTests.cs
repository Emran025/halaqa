using Halaqa.Desktop.Config.Persistence;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Time;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Auth;

public sealed class RestoreSessionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsAuthenticatedUserForUnexpiredProtectedSession()
    {
        var now = new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        var store = new FakeSessionStore(new AuthSession(
            Guid.NewGuid().ToString(), "Teacher", "معلم اختبار", "teacher@example.test", "token", now.AddMinutes(30)));

        var result = await new RestoreSessionUseCase(store, new FixedClock(now)).ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal("teacher@example.test", result.User.Email);
        Assert.Equal("token", result.AccessToken);
        Assert.False(store.Cleared);
    }

    [Fact]
    public async Task ExecuteAsync_ClearsExpiredSession()
    {
        var now = new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        var store = new FakeSessionStore(new AuthSession(
            Guid.NewGuid().ToString(), "Student", "طالب اختبار", "student@example.test", "token", now.AddMinutes(-1)));

        var result = await new RestoreSessionUseCase(store, new FixedClock(now)).ExecuteAsync();

        Assert.Null(result);
        Assert.True(store.Cleared);
    }

    private sealed class FixedClock : IClock
    {
        private readonly DateTimeOffset utcNow;

        public FixedClock(DateTimeOffset utcNow)
        {
            this.utcNow = utcNow;
        }

        public DateTimeOffset UtcNow => utcNow;
    }

    private sealed class FakeSessionStore : IAuthSessionStore
    {
        private readonly AuthSession? session;

        public FakeSessionStore(AuthSession? session)
        {
            this.session = session;
        }

        public bool Cleared { get; private set; }

        public Task<AuthSession?> ReadAsync(CancellationToken cancellationToken = default) => Task.FromResult(session);
        public Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Cleared = true;
            return Task.CompletedTask;
        }
    }
}
