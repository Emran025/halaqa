using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Sessions.Presentation;

public sealed class SessionTasksViewModelTests
{
    [Fact]
    public async Task Load_UsesSelectedSessionIdentifierAndDisplaysOfficialTasks()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = new SessionTasksViewModel(new ListSessionTasksUseCase(repository));
        var session = CreateSession();
        viewModel.Initialize(session);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(session.Id, repository.LastSessionId);
        Assert.Single(viewModel.Tasks);
        Assert.Equal(2, viewModel.CurrentPage);
        Assert.Equal(3, viewModel.LastPage);
        Assert.Equal(41, viewModel.Total);
        Assert.False(viewModel.IsError);
    }

    private static SessionListItem CreateSession()
    {
        var teacher = new SessionParticipant(Guid.NewGuid(), "teacher", "معلم اختبار", "teacher@example.test", null, "active");
        var student = new SessionParticipant(Guid.NewGuid(), "student", "طالب اختبار", "student@example.test", null, "active");
        return new SessionListItem(
            Guid.NewGuid(), Guid.NewGuid(), teacher, student, null, SessionTaskType.Memorization,
            OfficialSessionState.Accepted, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
            null, null, null, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
    }

    private sealed class FakeSessionTaskDirectoryRepository : ISessionTaskDirectoryRepository
    {
        public Guid LastSessionId { get; private set; }

        public Task<Result<SessionTaskPage>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            LastSessionId = sessionId;
            var task = new SessionTaskListItem(
                Guid.NewGuid(), sessionId, SessionTaskType.Memorization, 1,
                OfficialSessionTaskState.InProgress, 1, 2, 3, 1, "ملاحظة",
                80, 2, DateTimeOffset.UtcNow, null, null, null, 1);
            return Task.FromResult(Result<SessionTaskPage>.Success(new SessionTaskPage(new[] { task }, 2, 3, 20, 41)));
        }
    }
}
