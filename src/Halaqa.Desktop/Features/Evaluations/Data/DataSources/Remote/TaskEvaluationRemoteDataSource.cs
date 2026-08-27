using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Evaluations.Data.Models;
using Halaqa.Desktop.Features.Evaluations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Evaluations.Data.DataSources.Remote;

internal interface ITaskEvaluationRemoteDataSource
{
    Task<Result<TaskEvaluationResponseDto>> GetAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default);

    Task<Result<TaskEvaluationResponseDto>> UpsertAsync(UpsertTaskEvaluationCommand command, CancellationToken cancellationToken = default);
}

internal sealed class TaskEvaluationRemoteDataSource : ITaskEvaluationRemoteDataSource
{
    private readonly IApiClient apiClient;

    public TaskEvaluationRemoteDataSource(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<Result<TaskEvaluationResponseDto>> GetAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<TaskEvaluationResponseDto>($"sessions/{sessionId}/tasks/{taskId}/evaluation", cancellationToken);

    public Task<Result<TaskEvaluationResponseDto>> UpsertAsync(UpsertTaskEvaluationCommand command, CancellationToken cancellationToken = default)
    {
        var request = new UpsertTaskEvaluationRequestDto(command.Score, command.Comment);
        return apiClient.PutAsync<UpsertTaskEvaluationRequestDto, TaskEvaluationResponseDto>(
            $"sessions/{command.SessionId}/tasks/{command.TaskId}/evaluation",
            request,
            cancellationToken);
    }
}
