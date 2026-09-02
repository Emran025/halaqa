using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class PrepareLiveSessionUseCase
{

    private readonly ILiveSessionRepository repository;


    public PrepareLiveSessionUseCase(

        ILiveSessionRepository repository

    )

    {

        this.repository = repository;

    }

    public async Task<Result<(RealtimeSessionConfig Config, ChannelAuthorization Authorization)>> ExecuteAsync(
        Guid sessionId,
        string? clientConnectionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty)
        {
            return Result<(RealtimeSessionConfig, ChannelAuthorization)>.Failure(new AppError(
                AppErrorKind.Validation,
                "معرف الجلسة غير صالح."));
        }

        var configResult = await repository.GetRealtimeConfigAsync(sessionId, cancellationToken);
        if (!configResult.IsSuccess || configResult.Value is null)
        {
            return Result<(RealtimeSessionConfig, ChannelAuthorization)>.Failure(configResult.Error!);
        }

        var config = configResult.Value;
        if (!config.DirectP2POnly || config.IceCandidatePolicy != "host_only" || config.SignalingTransport != "laravel_websocket")
        {
            return Result<(RealtimeSessionConfig, ChannelAuthorization)>.Failure(new AppError(
                AppErrorKind.Forbidden,
                "إعداد الاتصال لا يطابق سياسة الاتصال المباشر المعتمدة."));
        }

        var authorizationResult = await repository.AuthorizeChannelAsync(
            sessionId,
            config.ChannelName,
            clientConnectionId,
            cancellationToken);
        if (!authorizationResult.IsSuccess || authorizationResult.Value is null)
        {
            return Result<(RealtimeSessionConfig, ChannelAuthorization)>.Failure(authorizationResult.Error!);
        }

        var authorization = authorizationResult.Value;
        if (!authorization.IsAuthorized || authorization.SessionId != sessionId || authorization.ChannelName != config.ChannelName)
        {
            return Result<(RealtimeSessionConfig, ChannelAuthorization)>.Failure(new AppError(
                AppErrorKind.Forbidden,
                "فشل تفويض قناة الجلسة الخاصة."));
        }

        return Result<(RealtimeSessionConfig, ChannelAuthorization)>.Success((config, authorization));
    }
}

public sealed class SaveOfficialMushafStateUseCase
{

    private readonly ILiveSessionRepository repository;


    public SaveOfficialMushafStateUseCase(

        ILiveSessionRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(
        Guid sessionId,
        int editionId,
        int pageNumber,
        int? ayahId,
        Guid clientOperationId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty || editionId < 1 || pageNumber is < 1 or > 604 || clientOperationId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure(new AppError(
                AppErrorKind.Validation,
                "لا يمكن تثبيت حالة مصحف غير صالحة.")));
        }

        return repository.SaveOfficialMushafStateAsync(
            sessionId,
            editionId,
            pageNumber,
            ayahId,
            clientOperationId,
            cancellationToken);
    }
}
