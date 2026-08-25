using Halaqa.Desktop.Features.Halaqas.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Halaqas.Data.Mappers;
using Halaqa.Desktop.Features.Halaqas.Data.Models;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Halaqas.Data.Repositories;

internal sealed class HalaqaRepository(IHalaqaRemoteDataSource remoteDataSource) : IHalaqaRepository
{
    public async Task<Result<HalaqaPage>> ListAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ListAsync(page, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<HalaqaPage>.Failure(result.Error ?? UnknownError());
        }
        return result.Value is null
            ? Result<HalaqaPage>.Failure(UnknownError())
            : HalaqaMapper.ToDomain(result.Value);
    }

    public async Task<Result<Halaqa>> CreateAsync(CreateHalaqaCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.CreateAsync(HalaqaMapper.ToDto(command), cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<Halaqa>> UpdateAsync(UpdateHalaqaCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateAsync(command.Id, HalaqaMapper.ToDto(command), cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<Halaqa>> ActivateAsync(Guid halaqaId, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ActivateAsync(halaqaId, cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<Halaqa>> DeactivateAsync(Guid halaqaId, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.DeactivateAsync(halaqaId, cancellationToken);
        return MapResponse(result);
    }

    private static Result<Halaqa> MapResponse(Result<HalaqaResponseDto> result)
    {
        if (!result.IsSuccess)
        {
            return Result<Halaqa>.Failure(result.Error ?? UnknownError());
        }
        return result.Value?.Halaqa is null
            ? Result<Halaqa>.Failure(UnknownError())
            : HalaqaMapper.ToDomain(result.Value.Halaqa);
    }

    private static AppError UnknownError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم استجابة حلقة فارغة أو غير متوقعة.");
}
