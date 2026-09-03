using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Sessions.Presentation;

public sealed class SessionsViewModelTests
{
    [Fact]
    public async Task Load_ForwardsOfficialFiltersAndDisplaysPaginatedSessions()
    {
        var repository = new FakeSessionDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize();
        viewModel.StateFilter = "connecting";
        viewModel.From = "2026-08-01 08:00 +03:00";
        viewModel.To = "2026-08-31 20:00 +03:00";

        await viewModel.LoadCommand.ExecuteAsync(null);

        var query = Assert.IsType<SessionQuery>(repository.LastQuery);
        Assert.Equal(OfficialSessionState.Connecting, query.State);
        Assert.Equal(new DateTimeOffset(2026, 8, 1, 8, 0, 0, TimeSpan.FromHours(3)), query.From);
        Assert.Equal(new DateTimeOffset(2026, 8, 31, 20, 0, 0, TimeSpan.FromHours(3)), query.To);
        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PerPage);
        Assert.Single(viewModel.Sessions);
        Assert.Equal(2, viewModel.CurrentPage);
        Assert.Equal(3, viewModel.LastPage);
        Assert.Equal(41, viewModel.Total);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task OpenTasks_RaisesTasksRequestedForSelectedOfficialSession()
    {
        var viewModel = CreateViewModel(new FakeSessionDirectoryRepository());
        viewModel.Initialize();
        await viewModel.LoadCommand.ExecuteAsync(null);
        var selected = Assert.IsType<SessionListItem>(viewModel.SelectedSession);
        SessionListItem? requested = null;
        viewModel.TasksRequested += (_, session) => requested = session;

        viewModel.OpenTasksCommand.Execute(null);

        Assert.Equal(selected, requested);
    }

    private static SessionsViewModel CreateViewModel(FakeSessionDirectoryRepository repository) =>
        new(
            new ListSessionsUseCase(repository),
            new AcceptLiveSessionUseCase(repository),
            new RejectLiveSessionUseCase(repository));

    private sealed class FakeSessionDirectoryRepository : ISessionDirectoryRepository
    {
        public SessionQuery? LastQuery { get; private set; }

        public Task<Result<SessionListItem>> CreateAsync(CreateLiveSessionCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<SessionListItem>.Failure(new AppError(AppErrorKind.Unknown, "غير مستخدم في الاختبار.")));

        public Task<Result<SessionListItem>> AcceptAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<SessionListItem>.Success(BuildSession(OfficialSessionState.Accepted)));

        public Task<Result<SessionListItem>> RejectAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<SessionListItem>.Success(BuildSession(OfficialSessionState.Rejected)));

        public Task<Result<SessionPage>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            var session = BuildSession(OfficialSessionState.Connecting);
            return Task.FromResult(Result<SessionPage>.Success(new SessionPage(new[] { session }, 2, 3, 20, 41)));
        }

        private static SessionListItem BuildSession(OfficialSessionState state)
        {
            var participant = new SessionParticipant(Guid.NewGuid(), "teacher", "معلم اختبار", "teacher@example.test", null, "active");
            var student = new SessionParticipant(Guid.NewGuid(), "student", "طالب اختبار", "student@example.test", null, "active");
            return new SessionListItem(
                Guid.NewGuid(),
                Guid.NewGuid(),
                participant,
                student,
                null,
                SessionTaskType.Memorization,
                state,
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow,
                null,
                null,
                null,
                null,
                true,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);
        }
    }
}
