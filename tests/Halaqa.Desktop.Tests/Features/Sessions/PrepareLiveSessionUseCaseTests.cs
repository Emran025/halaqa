using Xunit;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Tests.Features.Sessions;

public sealed class PrepareLiveSessionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UsesOnlyAuthorizedHostOnlyDirectConfiguration()
    {
        var sessionId = Guid.NewGuid();
        var repository = new FakeLiveSessionRepository
        {
            Config = new RealtimeSessionConfig(
                sessionId,
                $"private-live-session.{sessionId}",
                new Uri("wss://api.example.test/realtime"),
                DateTimeOffset.UtcNow.AddMinutes(5),
                DirectP2POnly: true,
                SignalingTransport: "laravel_websocket",
                IceCandidatePolicy: "host_only",
                MediaTransport: "webrtc_peer_to_peer"),
            Authorization = new ChannelAuthorization(
                IsAuthorized: true,
                sessionId,
                $"private-live-session.{sessionId}",
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddMinutes(5))
        };

        var result = await new PrepareLiveSessionUseCase(repository).ExecuteAsync(sessionId, "client-1");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(sessionId, result.Value.Value.Config.SessionId);
        Assert.Equal("client-1", repository.ReceivedClientConnectionId);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNonHostIcePolicyBeforeStartingMedia()
    {
        var sessionId = Guid.NewGuid();
        var repository = new FakeLiveSessionRepository
        {
            Config = new RealtimeSessionConfig(
                sessionId,
                $"private-live-session.{sessionId}",
                new Uri("wss://api.example.test/realtime"),
                DateTimeOffset.UtcNow.AddMinutes(5),
                DirectP2POnly: true,
                SignalingTransport: "laravel_websocket",
                IceCandidatePolicy: "relay_allowed",
                MediaTransport: "webrtc_peer_to_peer")
        };

        var result = await new PrepareLiveSessionUseCase(repository).ExecuteAsync(sessionId, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Forbidden, result.Error?.Kind);
        Assert.False(repository.AuthorizationRequested);
    }

    [Fact]
    public async Task SaveOfficialMushafStateUseCase_RejectsPageOutsideMushafRange()
    {
        var repository = new FakeLiveSessionRepository();
        var result = await new SaveOfficialMushafStateUseCase(repository).ExecuteAsync(
            Guid.NewGuid(),
            editionId: 1,
            pageNumber: 605,
            ayahId: null,
            clientOperationId: Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.False(repository.SaveMushafStateCalled);
    }

    private sealed class FakeLiveSessionRepository : ILiveSessionRepository
    {
        public RealtimeSessionConfig? Config { get; init; }
        public ChannelAuthorization? Authorization { get; init; }
        public string? ReceivedClientConnectionId { get; private set; }
        public bool AuthorizationRequested { get; private set; }
        public bool SaveMushafStateCalled { get; private set; }

        public Task<Result<RealtimeSessionConfig>> GetRealtimeConfigAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Config is null
                ? Result<RealtimeSessionConfig>.Failure(new AppError(AppErrorKind.NotFound, "الجلسة غير موجودة."))
                : Result<RealtimeSessionConfig>.Success(Config));

        public Task<Result<ChannelAuthorization>> AuthorizeChannelAsync(Guid sessionId, string channelName, string? clientConnectionId, CancellationToken cancellationToken = default)
        {
            AuthorizationRequested = true;
            ReceivedClientConnectionId = clientConnectionId;
            return Task.FromResult(Authorization is null
                ? Result<ChannelAuthorization>.Failure(new AppError(AppErrorKind.Forbidden, "رفض التفويض."))
                : Result<ChannelAuthorization>.Success(Authorization));
        }

        public Task<Result> SaveOfficialMushafStateAsync(Guid sessionId, int editionId, int pageNumber, int? ayahId, Guid clientOperationId, CancellationToken cancellationToken = default)
        {
            SaveMushafStateCalled = true;
            return Task.FromResult(Result.Success());
        }
    }
}
