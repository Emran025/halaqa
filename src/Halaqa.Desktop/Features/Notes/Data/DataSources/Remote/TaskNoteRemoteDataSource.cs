using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Notes.Data.Models;
using Halaqa.Desktop.Features.Notes.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notes.Data.DataSources.Remote;

internal interface ITaskNoteRemoteDataSource
{
    Task<Result<TaskNoteCollectionResponseDto>> ListAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default);

    Task<Result<TaskNoteResponseDto>> CreateAsync(CreateTaskNoteCommand command, CancellationToken cancellationToken = default);

    Task<Result<TaskNoteResponseDto>> UpdateAsync(UpdateTaskNoteCommand command, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(DeleteTaskNoteCommand command, CancellationToken cancellationToken = default);
}

internal sealed class TaskNoteRemoteDataSource : ITaskNoteRemoteDataSource
{
    private readonly IApiClient apiClient;

    public TaskNoteRemoteDataSource(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<Result<TaskNoteCollectionResponseDto>> ListAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<TaskNoteCollectionResponseDto>($"sessions/{sessionId}/tasks/{taskId}/notes", cancellationToken);

    public Task<Result<TaskNoteResponseDto>> CreateAsync(CreateTaskNoteCommand command, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<CreateTaskNoteRequestDto, TaskNoteResponseDto>(
            $"sessions/{command.SessionId}/tasks/{command.TaskId}/notes",
            new CreateTaskNoteRequestDto(command.Body, command.ClientOperationId),
            cancellationToken);

    public Task<Result<TaskNoteResponseDto>> UpdateAsync(UpdateTaskNoteCommand command, CancellationToken cancellationToken = default) =>
        apiClient.PatchAsync<UpdateTaskNoteRequestDto, TaskNoteResponseDto>(
            $"sessions/{command.SessionId}/tasks/{command.TaskId}/notes/{command.NoteId}",
            new UpdateTaskNoteRequestDto(command.Body),
            cancellationToken);

    public Task<Result> DeleteAsync(DeleteTaskNoteCommand command, CancellationToken cancellationToken = default) =>
        apiClient.DeleteAsync($"sessions/{command.SessionId}/tasks/{command.TaskId}/notes/{command.NoteId}", cancellationToken);
}
