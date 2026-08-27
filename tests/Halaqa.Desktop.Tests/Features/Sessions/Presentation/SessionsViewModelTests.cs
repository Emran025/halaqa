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
        var viewModel = new SessionsViewModel(new ListSessionsUseCase(repository));
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

    private sealed class FakeSessionDirectoryRepository : ISessionDirectoryRepository
    {
        public SessionQuery? LastQuery { get; private set; }

        public Task<Result<SessionPage>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            var participant = new SessionParticipant(Guid.NewGuid(), "teacher", "معلم اختبار", "teacher@example.test", null, "active");
            var student = new SessionParticipant(Guid.NewGuid(), "student", "طالب اختبار", "student@example.test", null, "active");
            var session = new SessionListItem(
                Guid.NewGuid(),
                Guid.NewGuid(),
                participant,
                student,
                null,
                SessionTaskType.Memorization,
                OfficialSessionState.Connecting,
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow,
                null,
                null,
                null,
                null,
                true,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);
            return Task.FromResult(Result<SessionPage>.Success(new SessionPage(new[] { session }, 2, 3, 20, 41)));
        }
    }
}
