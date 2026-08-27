using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;

internal interface ISessionTaskDirectoryRemoteDataSource
{
    Task<Result<SessionTaskCollectionResponseDto>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Result<SessionTaskResponseDto>> CreateAsync(CreateSessionTaskCommand command, CancellationToken cancellationToken = default);
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
        var request = new CreateSessionTaskRequestDto(command.TaskType.ToString().ToLowerInvariant(), command.ClientOperationId);
        return apiClient.PostAsync<CreateSessionTaskRequestDto, SessionTaskResponseDto>($"sessions/{command.SessionId}/tasks", request, cancellationToken);
    }
}
