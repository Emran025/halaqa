using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.Stores;
using Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Sessions;

public sealed class LiveSessionViewModelTests
{
    [Fact]
    public async Task InitializeMushaf_LoadsLocalPageAndSetsLocalPresence()
    {
        var viewModel = CreateViewModel(new FakeQuranRepository());

        await viewModel.InitializeMushafAsync();

        Assert.NotNull(viewModel.QuranPage);
        Assert.Equal(1, viewModel.QuranPage!.PageNumber);
        Assert.True(viewModel.QuranPage.IsFromLocalCache);
        Assert.Equal("ﱁﱂﱃﱄﱅ", Assert.Single(viewModel.QuranPage.Ayahs).PageGlyphText);
    }

    private static LiveSessionViewModel CreateViewModel(
        IQuranRepository quranRepository,
        FakeMushafRealtimeChannel? mushafRealtimeChannel = null) => new(
        new LiveSessionStore(),
        new FakePeerMediaConnection(),
        mushafRealtimeChannel ?? new FakeMushafRealtimeChannel(),
        new FakeLocalVideoRecorder(),
        new SaveOfficialMushafStateUseCase(new FakeLiveSessionRepository()),
        new GetQuranPageUseCase(quranRepository),
        new GetQuranIndexUseCase(quranRepository));

    private sealed class FakeQuranRepository : IQuranRepository
    {
        public Task<Result<QuranPage>> GetPageAsync(
            int editionId,
            int pageNumber,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<QuranPage>.Success(new QuranPage(
                editionId,
                pageNumber,
                new[] { new QuranSurah(1, editionId, 1, "الفاتحة", 7, "مكية") },
                new[] { new QuranAyah(1, editionId, 1, 1, pageNumber, "بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ", "ﱁﱂﱃﱄﱅ", 1, new[] { new QuranWord(0, "ﱁ") }) },
                true)));

        public Task<Result<IReadOnlyList<QuranSurahIndexItem>>> GetSurahsIndexAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<QuranSurahIndexItem>>.Success(new[]
            {
                new QuranSurahIndexItem(1, "الفاتحة", 7, 1, "مكية")
            }));

        public Task<Result<IReadOnlyList<QuranJuzIndexItem>>> GetJuzIndexAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<QuranJuzIndexItem>>.Success(new[]
            {
                new QuranJuzIndexItem(1, "الجزء 1", 1, 21)
            }));
    }

    private sealed class FakeLiveSessionRepository : ILiveSessionRepository
    {
        public Task<Result<RealtimeSessionConfig>> GetRealtimeConfigAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RealtimeSessionConfig>.Failure(new AppError(AppErrorKind.Unknown, "غير مستخدم.")));

        public Task<Result<ChannelAuthorization>> AuthorizeChannelAsync(Guid sessionId, string channelName, string? clientConnectionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<ChannelAuthorization>.Failure(new AppError(AppErrorKind.Unknown, "غير مستخدم.")));

        public Task<Result> SaveOfficialMushafStateAsync(Guid sessionId, int editionId, int pageNumber, int? ayahId, Guid clientOperationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class FakePeerMediaConnection : IPeerMediaConnection
    {
        public event EventHandler<PeerConnectionStateChangedEventArgs>? StateChanged;
        public event EventHandler<PeerMediaStateChangedEventArgs>? RemoteMediaStateChanged;
        public Task InitializeAsync(RealtimeSessionConfig config, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CreateOfferAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task HandleOfferAsync(string sdp, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task HandleAnswerAsync(string sdp, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task HandleHostIceCandidateAsync(HostIceCandidate candidate, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetMicrophoneMutedAsync(bool isMuted, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetCameraEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeMushafRealtimeChannel : IMushafRealtimeChannel
    {
        public bool PresenceSent { get; private set; }
        public event EventHandler<MushafPresenceState>? PresenceReceived;
        public event EventHandler<PeerRepeatRequest>? RepeatRequested;
        public Task SendPresenceAsync(MushafPresenceState state, CancellationToken cancellationToken = default)
        {
            PresenceSent = true;
            return Task.CompletedTask;
        }
        public Task SendRepeatRequestAsync(PeerRepeatRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeLocalVideoRecorder : ILocalVideoRecorder
    {
        public event EventHandler<LocalRecordingState>? StateChanged;
        public Task StartAsync(string outputDirectory, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
