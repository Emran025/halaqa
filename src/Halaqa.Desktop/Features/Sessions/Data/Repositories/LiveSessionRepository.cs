using Halaqa.Desktop.Features.Sessions.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.Repositories;

internal sealed class LiveSessionRepository(ILiveSessionRemoteDataSource remoteDataSource) : ILiveSessionRepository
{
    public async Task<Result<RealtimeSessionConfig>> GetRealtimeConfigAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.GetRealtimeConfigAsync(sessionId, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return Result<RealtimeSessionConfig>.Failure(result.Error!);
        }

        var realtime = result.Value.RealtimeSession;
        return Result<RealtimeSessionConfig>.Success(new RealtimeSessionConfig(
            realtime.SessionId,
            realtime.ChannelName,
            realtime.WebSocketUrl,
            realtime.ExpiresAt,
            realtime.DirectP2POnly,
            realtime.SignalingTransport,
            realtime.IceCandidatePolicy,
            realtime.MediaTransport));
    }

    public async Task<Result<ChannelAuthorization>> AuthorizeChannelAsync(
        Guid sessionId,
        string channelName,
        string? clientConnectionId,
        CancellationToken cancellationToken = default)
    {
        var request = new AuthorizeRealtimeChannelRequestDto(sessionId, channelName, clientConnectionId);
        var result = await remoteDataSource.AuthorizeChannelAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return Result<ChannelAuthorization>.Failure(result.Error!);
        }

        var authorization = result.Value.Authorization;
        return Result<ChannelAuthorization>.Success(new ChannelAuthorization(
            authorization.Authorized,
            authorization.SessionId,
            authorization.ChannelName,
            authorization.RecipientId,
            authorization.ExpiresAt));
    }

    public Task<Result> SaveOfficialMushafStateAsync(
        Guid sessionId,
        int editionId,
        int pageNumber,
        int? ayahId,
        Guid clientOperationId,
        CancellationToken cancellationToken = default) =>
        remoteDataSource.SaveMushafStateAsync(
            sessionId,
            new SaveMushafStateRequestDto(editionId, pageNumber, ayahId, clientOperationId),
            cancellationToken);
}
