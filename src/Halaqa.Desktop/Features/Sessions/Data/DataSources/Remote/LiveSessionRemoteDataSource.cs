using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;

internal interface ILiveSessionRemoteDataSource
{
    Task<Result<RealtimeSessionResponseDto>> GetRealtimeConfigAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<RealtimeChannelAuthorizationResponseDto>> AuthorizeChannelAsync(
        AuthorizeRealtimeChannelRequestDto request,
        CancellationToken cancellationToken = default);
    Task<Result> SaveMushafStateAsync(Guid sessionId, SaveMushafStateRequestDto request, CancellationToken cancellationToken = default);
}

internal sealed class LiveSessionRemoteDataSource(IApiClient apiClient) : ILiveSessionRemoteDataSource
{
    public Task<Result<RealtimeSessionResponseDto>> GetRealtimeConfigAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<RealtimeSessionResponseDto>($"sessions/{sessionId}/realtime", cancellationToken);

    public Task<Result<RealtimeChannelAuthorizationResponseDto>> AuthorizeChannelAsync(
        AuthorizeRealtimeChannelRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<AuthorizeRealtimeChannelRequestDto, RealtimeChannelAuthorizationResponseDto>(
            "realtime/channels/authorize",
            request,
            cancellationToken);

    public Task<Result> SaveMushafStateAsync(
        Guid sessionId,
        SaveMushafStateRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PutAsync($"sessions/{sessionId}/mushaf-state", request, cancellationToken);
}
