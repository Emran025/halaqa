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
        var viewModel = CreateViewModel(repository);
        var session = CreateSession();
        viewModel.Initialize(session, canCreateTasks: false, canReportMistakes: true);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(session.Id, repository.LastSessionId);
        Assert.Single(viewModel.Tasks);
        Assert.Equal(2, viewModel.CurrentPage);
        Assert.Equal(3, viewModel.LastPage);
        Assert.Equal(41, viewModel.Total);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task CreateTask_ForTeacherSendsMinimumContractCommandAndRefreshesList()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        var session = CreateSession();
        viewModel.Initialize(session, canCreateTasks: true, canReportMistakes: true);
        viewModel.NewTaskType = SessionTaskType.Recitation;

        await viewModel.CreateTaskCommand.ExecuteAsync(null);

        var command = Assert.IsType<CreateSessionTaskCommand>(repository.LastCreateCommand);
        Assert.Equal(session.Id, command.SessionId);
        Assert.Equal(SessionTaskType.Recitation, command.TaskType);
        Assert.NotEqual(Guid.Empty, command.ClientOperationId);
        Assert.Equal(session.Id, repository.LastSessionId);
        Assert.Single(viewModel.Tasks);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task ReportMistake_RaisesEventForSelectedTaskWhenInitializedForAuthorizedParticipant()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(CreateSession(), canCreateTasks: true, canReportMistakes: true);
        await viewModel.LoadCommand.ExecuteAsync(null);
        var selected = Assert.IsType<SessionTaskListItem>(viewModel.SelectedTask);
        SessionTaskListItem? requested = null;
        viewModel.MistakeReportingRequested += (_, task) => requested = task;

        viewModel.ReportMistakeCommand.Execute(null);

        Assert.Equal(selected, requested);
    }

    [Fact]
    public void CreateTask_IsUnavailableWhenScreenIsInitializedForStudent()
    {
        var viewModel = CreateViewModel(new FakeSessionTaskDirectoryRepository());
        viewModel.Initialize(CreateSession(), canCreateTasks: false, canReportMistakes: true);

        Assert.False(viewModel.CreateTaskCommand.CanExecute(null));
    }

    private static SessionTasksViewModel CreateViewModel(FakeSessionTaskDirectoryRepository repository) =>
        new(new ListSessionTasksUseCase(repository), new CreateSessionTaskUseCase(repository));

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
        public CreateSessionTaskCommand? LastCreateCommand { get; private set; }

        public Task<Result<SessionTaskPage>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            LastSessionId = sessionId;
            return Task.FromResult(Result<SessionTaskPage>.Success(new SessionTaskPage(new[] { CreateTask(sessionId) }, 2, 3, 20, 41)));
        }

        public Task<Result<SessionTaskListItem>> CreateAsync(CreateSessionTaskCommand command, CancellationToken cancellationToken = default)
        {
            LastCreateCommand = command;
            return Task.FromResult(Result<SessionTaskListItem>.Success(CreateTask(command.SessionId, command.TaskType)));
        }

        private static SessionTaskListItem CreateTask(Guid sessionId, SessionTaskType taskType = SessionTaskType.Memorization) =>
            new(
                Guid.NewGuid(), sessionId, taskType, 1,
                OfficialSessionTaskState.InProgress, 1, 2, 3, 1, "ملاحظة",
                80, 2, DateTimeOffset.UtcNow, null, null, null, 1);
    }
}
