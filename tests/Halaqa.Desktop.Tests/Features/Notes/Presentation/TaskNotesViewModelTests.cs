using Halaqa.Desktop.Features.Notes.Domain.Entities;
using Halaqa.Desktop.Features.Notes.Domain.Repositories;
using Halaqa.Desktop.Features.Notes.Domain.UseCases;
using Halaqa.Desktop.Features.Notes.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Notes.Presentation;

public sealed class TaskNotesViewModelTests
{
    [Fact]
    public async Task Load_DisplaysOfficialNotesAndSelectsCurrentUsersNote()
    {
        var currentUserId = Guid.NewGuid();
        var repository = new FakeTaskNoteRepository(currentUserId);
        var viewModel = CreateViewModel(repository);
        var sessionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        viewModel.Initialize(sessionId, taskId, "مهمة اختبار", currentUserId);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(sessionId, repository.LastSessionId);
        Assert.Equal(taskId, repository.LastTaskId);
        Assert.Equal(2, viewModel.Notes.Count);
        Assert.NotNull(viewModel.SelectedNote);
        Assert.Equal(currentUserId, viewModel.SelectedNote!.Author.Id);
        Assert.Equal("ملاحظتي", viewModel.EditBody);
    }

    [Fact]
    public async Task Create_GeneratesOperationIdAndSendsBody()
    {
        var currentUserId = Guid.NewGuid();
        var repository = new FakeTaskNoteRepository(currentUserId);
        var viewModel = CreateViewModel(repository);
        var sessionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        viewModel.Initialize(sessionId, taskId, "مهمة اختبار", currentUserId);
        viewModel.NewBody = "استفسار تعليمي";

        await viewModel.CreateCommand.ExecuteAsync(null);

        var command = Assert.IsType<CreateTaskNoteCommand>(repository.LastCreateCommand);
        Assert.Equal(sessionId, command.SessionId);
        Assert.Equal(taskId, command.TaskId);
        Assert.Equal("استفسار تعليمي", command.Body);
        Assert.NotEqual(Guid.Empty, command.ClientOperationId);
        Assert.Equal("تمت إضافة الملاحظة من الخادم.", viewModel.Message);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Update_UpdatesCurrentUsersSelectedNote()
    {
        var currentUserId = Guid.NewGuid();
        var repository = new FakeTaskNoteRepository(currentUserId);
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", currentUserId);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.EditBody = "تعديل الملاحظة";

        await viewModel.UpdateCommand.ExecuteAsync(null);

        var command = Assert.IsType<UpdateTaskNoteCommand>(repository.LastUpdateCommand);
        Assert.Equal(currentUserId, viewModel.SelectedNote!.Author.Id);
        Assert.Equal("تعديل الملاحظة", command.Body);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Commands_DoNotModifyAnotherAuthorsSelectedNote()
    {
        var currentUserId = Guid.NewGuid();
        var repository = new FakeTaskNoteRepository(currentUserId);
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", currentUserId);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedNote = viewModel.Notes.Single(note => note.Author.Id != currentUserId);

        Assert.False(viewModel.UpdateCommand.CanExecute(null));
        Assert.False(viewModel.DeleteCommand.CanExecute(null));
    }

    [Fact]
    public async Task Create_WithEmptyBodyDoesNotCallRepository()
    {
        var currentUserId = Guid.NewGuid();
        var repository = new FakeTaskNoteRepository(currentUserId);
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", currentUserId);
        viewModel.NewBody = " ";

        await viewModel.CreateCommand.ExecuteAsync(null);

        Assert.Null(repository.LastCreateCommand);
        Assert.True(viewModel.IsError);
        Assert.Equal("نص الملاحظة مطلوب.", viewModel.Message);
    }

    private static TaskNotesViewModel CreateViewModel(FakeTaskNoteRepository repository) =>
        new(
            new ListTaskNotesUseCase(repository),
            new CreateTaskNoteUseCase(repository),
            new UpdateTaskNoteUseCase(repository),
            new DeleteTaskNoteUseCase(repository));

    private sealed class FakeTaskNoteRepository : ITaskNoteRepository
    {
        private readonly Guid currentUserId;

        public FakeTaskNoteRepository(Guid currentUserId)
        {
            this.currentUserId = currentUserId;
        }

        public Guid LastSessionId { get; private set; }
        public Guid LastTaskId { get; private set; }
        public CreateTaskNoteCommand? LastCreateCommand { get; private set; }
        public UpdateTaskNoteCommand? LastUpdateCommand { get; private set; }

        public Task<Result<TaskNotePage>> ListAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default)
        {
            LastSessionId = sessionId;
            LastTaskId = taskId;
            return Task.FromResult(Result<TaskNotePage>.Success(new TaskNotePage(new[]
            {
                new TaskNote(Guid.NewGuid(), "ملاحظة من الطرف الآخر", new TaskNoteAuthor(Guid.NewGuid(), "طرف آخر"), DateTimeOffset.UtcNow, null),
                new TaskNote(Guid.NewGuid(), "ملاحظتي", new TaskNoteAuthor(currentUserId, "المستخدم الحالي"), DateTimeOffset.UtcNow, null)
            })));
        }

        public Task<Result<TaskNote>> CreateAsync(CreateTaskNoteCommand command, CancellationToken cancellationToken = default)
        {
            LastCreateCommand = command;
            return Task.FromResult(Result<TaskNote>.Success(CreateOwnNote(command.Body)));
        }

        public Task<Result<TaskNote>> UpdateAsync(UpdateTaskNoteCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateCommand = command;
            return Task.FromResult(Result<TaskNote>.Success(CreateOwnNote(command.Body)));
        }

        public Task<Result> DeleteAsync(DeleteTaskNoteCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

        private TaskNote CreateOwnNote(string body) =>
            new(Guid.NewGuid(), body, new TaskNoteAuthor(currentUserId, "المستخدم الحالي"), DateTimeOffset.UtcNow, null);
    }
}
