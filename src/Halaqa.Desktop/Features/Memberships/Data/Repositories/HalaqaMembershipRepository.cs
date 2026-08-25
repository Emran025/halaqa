using Halaqa.Desktop.Features.Memberships.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Memberships.Data.Mappers;
using Halaqa.Desktop.Features.Memberships.Data.Models;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Memberships.Data.Repositories;

internal sealed class HalaqaMembershipRepository(IHalaqaMembershipRemoteDataSource remoteDataSource) : IHalaqaMembershipRepository
{
    public async Task<Result<MembershipPage>> ListAsync(
        Guid halaqaId,
        string? status = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ListAsync(halaqaId, status, page, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<MembershipPage>.Failure(result.Error ?? UnknownError());
        }
        return result.Value is null
            ? Result<MembershipPage>.Failure(UnknownError())
            : HalaqaMembershipMapper.ToDomain(result.Value);
    }

    public async Task<Result<HalaqaMembership>> AssignAsync(
        AssignStudentToHalaqaCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.AssignAsync(
            command.HalaqaId,
            HalaqaMembershipMapper.ToDto(command),
            cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<HalaqaMembership>> UpdateAsync(
        UpdateHalaqaMembershipCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateAsync(
            command.HalaqaId,
            command.MembershipId,
            HalaqaMembershipMapper.ToDto(command),
            cancellationToken);
        return MapResponse(result);
    }

    public Task<Result> RemoveAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default) =>
        remoteDataSource.RemoveAsync(halaqaId, membershipId, cancellationToken);

    private static Result<HalaqaMembership> MapResponse(Result<MembershipResponseDto> result)
    {
        if (!result.IsSuccess)
        {
            return Result<HalaqaMembership>.Failure(result.Error ?? UnknownError());
        }
        return result.Value?.Membership is null
            ? Result<HalaqaMembership>.Failure(UnknownError())
            : HalaqaMembershipMapper.ToDomain(result.Value.Membership);
    }

    private static AppError UnknownError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم استجابة عضوية فارغة أو غير متوقعة.");
}
