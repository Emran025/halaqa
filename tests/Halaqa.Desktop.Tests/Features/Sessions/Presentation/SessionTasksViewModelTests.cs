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
    public async Task CreateTask_ForTeacherSendsAllEnteredContractFieldsAndRefreshesList()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        var session = CreateSession();
        viewModel.Initialize(session, canCreateTasks: true, canReportMistakes: true);
        viewModel.NewTaskType = SessionTaskType.Recitation;
        viewModel.NewSequenceNo = "3";
        viewModel.NewPlannedAmount = "2.5";
        viewModel.NewPlannedFromUnitId = "10";
        viewModel.NewPlannedToUnitId = "15";
        viewModel.NewStartPage = "2";
        viewModel.NewStartAyahId = "8";
        viewModel.NewEndPage = "4";
        viewModel.NewEndAyahId = "20";

        await viewModel.CreateTaskCommand.ExecuteAsync(null);

        var command = Assert.IsType<CreateSessionTaskCommand>(repository.LastCreateCommand);
        Assert.Equal(session.Id, command.SessionId);
        Assert.Equal(SessionTaskType.Recitation, command.TaskType);
        Assert.NotEqual(Guid.Empty, command.ClientOperationId);
        Assert.Equal(3, command.SequenceNo);
        Assert.Equal(2.5m, command.PlannedAmount);
        Assert.Equal(10, command.PlannedFromUnitId);
        Assert.Equal(15, command.PlannedToUnitId);
        Assert.Equal(2, command.StartPage);
        Assert.Equal(8, command.StartAyahId);
        Assert.Equal(4, command.EndPage);
        Assert.Equal(20, command.EndAyahId);
        Assert.Equal(session.Id, repository.LastSessionId);
        Assert.Single(viewModel.Tasks);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task UpdateTask_ForTeacherSendsOnlyEnteredPatchFieldsForSelectedTask()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        var session = CreateSession();
        viewModel.Initialize(session, canCreateTasks: true, canReportMistakes: true);
        await viewModel.LoadCommand.ExecuteAsync(null);
        var selected = Assert.IsType<SessionTaskListItem>(viewModel.SelectedTask);
        viewModel.UpdateCurrentPage = "5";
        viewModel.UpdateCurrentAyahId = "33";
        viewModel.UpdateActualAmount = "1.75";
        viewModel.SelectedUpdateState = OfficialSessionTaskState.Completed;

        await viewModel.UpdateTaskCommand.ExecuteAsync(null);

        var command = Assert.IsType<UpdateSessionTaskCommand>(repository.LastUpdateCommand);
        Assert.Equal(session.Id, command.SessionId);
        Assert.Equal(selected.Id, command.TaskId);
        Assert.Equal(5, command.CurrentPage);
        Assert.Equal(33, command.CurrentAyahId);
        Assert.Equal(1.75m, command.ActualAmount);
        Assert.Equal(OfficialSessionTaskState.Completed, command.State);
        Assert.Null(command.StartPage);
        Assert.Null(command.PlannedAmount);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task UpdateTask_WithInvalidPageDoesNotCallRepository()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(CreateSession(), canCreateTasks: true, canReportMistakes: true);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.UpdateCurrentPage = "605";

        await viewModel.UpdateTaskCommand.ExecuteAsync(null);

        Assert.Null(repository.LastUpdateCommand);
        Assert.True(viewModel.IsError);
        Assert.Equal("الصفحة الحالية يجب أن يكون بين 1 و604.", viewModel.Message);
    }

    [Fact]
    public async Task SaveDraft_ForAuthorizedParticipantSendsUniqueOperationAndOptionalPosition()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        var session = CreateSession();
        viewModel.Initialize(session, canCreateTasks: false, canReportMistakes: true);
        await viewModel.LoadCommand.ExecuteAsync(null);
        var selected = Assert.IsType<SessionTaskListItem>(viewModel.SelectedTask);
        viewModel.DraftCurrentPage = "12";
        viewModel.DraftCurrentAyahId = "155";

        await viewModel.SaveDraftCommand.ExecuteAsync(null);

        var command = Assert.IsType<SaveSessionTaskDraftCommand>(repository.LastSaveDraftCommand);
        Assert.Equal(session.Id, command.SessionId);
        Assert.Equal(selected.Id, command.TaskId);
        Assert.NotEqual(Guid.Empty, command.ClientOperationId);
        Assert.Equal(12, command.CurrentPage);
        Assert.Equal(155, command.CurrentAyahId);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task SaveDraft_WithInvalidPositionDoesNotCallRepository()
    {
        var repository = new FakeSessionTaskDirectoryRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(CreateSession(), canCreateTasks: false, canReportMistakes: true);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.DraftCurrentPage = "605";

        await viewModel.SaveDraftCommand.ExecuteAsync(null);

        Assert.Null(repository.LastSaveDraftCommand);
        Assert.True(viewModel.IsError);
        Assert.Equal("الصفحة الحالية يجب أن يكون بين 1 و604.", viewModel.Message);
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
    public async Task TeacherOnlyTaskCommands_AreUnavailableWhenScreenIsInitializedForStudent()
    {
        var viewModel = CreateViewModel(new FakeSessionTaskDirectoryRepository());
        viewModel.Initialize(CreateSession(), canCreateTasks: false, canReportMistakes: true);
        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.CreateTaskCommand.CanExecute(null));
        Assert.False(viewModel.UpdateTaskCommand.CanExecute(null));
        Assert.True(viewModel.ReportMistakeCommand.CanExecute(null));
        Assert.True(viewModel.SaveDraftCommand.CanExecute(null));
    }

    private static SessionTasksViewModel CreateViewModel(FakeSessionTaskDirectoryRepository repository) =>
        new(
            new ListSessionTasksUseCase(repository),
            new CreateSessionTaskUseCase(repository),
            new UpdateSessionTaskUseCase(repository),
            new SaveSessionTaskDraftUseCase(repository));

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
        public UpdateSessionTaskCommand? LastUpdateCommand { get; private set; }
        public SaveSessionTaskDraftCommand? LastSaveDraftCommand { get; private set; }

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

        public Task<Result<SessionTaskListItem>> SaveDraftAsync(SaveSessionTaskDraftCommand command, CancellationToken cancellationToken = default)
        {
            LastSaveDraftCommand = command;
            return Task.FromResult(Result<SessionTaskListItem>.Success(CreateTask(command.SessionId)));
        }

        public Task<Result<SessionTaskListItem>> UpdateAsync(UpdateSessionTaskCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateCommand = command;
            return Task.FromResult(Result<SessionTaskListItem>.Success(CreateTask(command.SessionId)));
        }

        private static SessionTaskListItem CreateTask(Guid sessionId, SessionTaskType taskType = SessionTaskType.Memorization) =>
            new(
                Guid.NewGuid(), sessionId, taskType, 1,
                OfficialSessionTaskState.InProgress, 1, 2, 3, 1, "ملاحظة",
                80, 2, DateTimeOffset.UtcNow, null, null, null, 1);
    }
}
