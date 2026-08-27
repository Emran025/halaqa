using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;

internal interface ISessionTaskDirectoryRemoteDataSource
{
    Task<Result<SessionTaskCollectionResponseDto>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Result<SessionTaskResponseDto>> CreateAsync(CreateSessionTaskCommand command, CancellationToken cancellationToken = default);

    Task<Result<SessionTaskResponseDto>> UpdateAsync(UpdateSessionTaskCommand command, CancellationToken cancellationToken = default);

    Task<Result<SessionTaskResponseDto>> SaveDraftAsync(SaveSessionTaskDraftCommand command, CancellationToken cancellationToken = default);
}

internal sealed class SessionTaskDirectoryRemoteDataSource : ISessionTaskDirectoryRemoteDataSource
{
    private readonly IApiClient apiClient;

    public SessionTaskDirectoryRemoteDataSource(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<Result<SessionTaskCollectionResponseDto>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<SessionTaskCollectionResponseDto>($"sessions/{sessionId}/tasks", cancellationToken);

    public Task<Result<SessionTaskResponseDto>> CreateAsync(CreateSessionTaskCommand command, CancellationToken cancellationToken = default)
    {
        var request = new CreateSessionTaskRequestDto(
            command.TaskType.ToString().ToLowerInvariant(),
            command.ClientOperationId,
            command.SequenceNo,
            command.PlannedAmount,
            command.PlannedFromUnitId,
            command.PlannedToUnitId,
            command.StartPage,
            command.StartAyahId,
            command.EndPage,
            command.EndAyahId);
        return apiClient.PostAsync<CreateSessionTaskRequestDto, SessionTaskResponseDto>($"sessions/{command.SessionId}/tasks", request, cancellationToken);
    }

    public Task<Result<SessionTaskResponseDto>> SaveDraftAsync(SaveSessionTaskDraftCommand command, CancellationToken cancellationToken = default)
    {
        var request = new SaveSessionTaskDraftRequestDto(command.ClientOperationId, command.CurrentPage, command.CurrentAyahId);
        return apiClient.PostAsync<SaveSessionTaskDraftRequestDto, SessionTaskResponseDto>(
            $"sessions/{command.SessionId}/tasks/{command.TaskId}/save-draft",
            request,
            cancellationToken);
    }

    public Task<Result<SessionTaskResponseDto>> UpdateAsync(UpdateSessionTaskCommand command, CancellationToken cancellationToken = default)
    {
        var request = new UpdateSessionTaskRequestDto(
            command.PlannedFromUnitId,
            command.PlannedToUnitId,
            command.StartPage,
            command.StartAyahId,
            command.EndPage,
            command.EndAyahId,
            command.CurrentPage,
            command.CurrentAyahId,
            command.State?.ToString().ToLowerInvariant(),
            command.PlannedAmount,
            command.ActualAmount);
        return apiClient.PatchAsync<UpdateSessionTaskRequestDto, SessionTaskResponseDto>(
            $"sessions/{command.SessionId}/tasks/{command.TaskId}",
            request,
            cancellationToken);
    }
}
