using Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Profile.Data.Mappers;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.Repositories;

internal sealed class ProfileRepository : IProfileRepository
{

    private readonly IProfileRemoteDataSource remoteDataSource;


    public ProfileRepository(

        IProfileRemoteDataSource remoteDataSource

    )

    {

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<UserProfile>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.GetCurrentAsync(cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<UserProfile>> UpdateCurrentAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateCurrentAsync(ProfileMapper.ToDto(command), cancellationToken);
        return MapResponse(result);
    }

    private static Result<UserProfile> MapResponse(Result<UserProfileResponseDto> result)
    {
        if (!result.IsSuccess)
        {
            return Result<UserProfile>.Failure(result.Error ?? new AppError(
                AppErrorKind.Unknown,
                "تعذر تفسير استجابة الخادم."));
        }

        if (result.Value is null)
        {
            return Result<UserProfile>.Failure(new AppError(
                AppErrorKind.Unknown,
                "أعاد الخادم استجابة ملف فارغة أو غير متوقعة."));
        }

        return ProfileMapper.ToDomain(result.Value.User);
    }
}
