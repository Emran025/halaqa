using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;

internal interface ISessionDirectoryRemoteDataSource
{
    Task<Result<SessionResponseDto>> CreateAsync(CreateLiveSessionCommand command, CancellationToken cancellationToken = default);
    Task<Result<SessionResponseDto>> AcceptAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<SessionResponseDto>> RejectAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<SessionCollectionResponseDto>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default);
}

internal sealed class SessionDirectoryRemoteDataSource : ISessionDirectoryRemoteDataSource
{
    private readonly IApiClient apiClient;

    public SessionDirectoryRemoteDataSource(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<Result<SessionResponseDto>> CreateAsync(CreateLiveSessionCommand command, CancellationToken cancellationToken = default)
    {
        var request = new CreateSessionRequestDto(
            command.HalaqaId,
            command.StudentId,
            command.FollowUpItemId,
            ToContractValue(command.TaskType),
            command.ScheduledAt,
            command.ClientOperationId);
        return apiClient.PostAsync<CreateSessionRequestDto, SessionResponseDto>("sessions", request, cancellationToken);
    }

    public Task<Result<SessionResponseDto>> AcceptAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        apiClient.PostEmptyAsync<SessionResponseDto>($"sessions/{sessionId}/accept", cancellationToken);

    public Task<Result<SessionResponseDto>> RejectAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        apiClient.PostEmptyAsync<SessionResponseDto>($"sessions/{sessionId}/reject", cancellationToken);

    public Task<Result<SessionCollectionResponseDto>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>
        {
            $"page={query.Page}",
            $"per_page={query.PerPage}"
        };
        if (query.HalaqaId is { } halaqaId)
        {
            parameters.Add($"halaqa_id={halaqaId}");
        }
        if (query.StudentId is { } studentId)
        {
            parameters.Add($"student_id={studentId}");
        }
        if (query.State is { } state)
        {
            parameters.Add($"state={Uri.EscapeDataString(ToContractValue(state))}");
        }
        if (query.From is { } from)
        {
            parameters.Add($"from={Uri.EscapeDataString(from.ToString("O"))}");
        }
        if (query.To is { } to)
        {
            parameters.Add($"to={Uri.EscapeDataString(to.ToString("O"))}");
        }

        return apiClient.GetAsync<SessionCollectionResponseDto>($"sessions?{string.Join("&", parameters)}", cancellationToken);
    }

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
