using Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Profile.Data.Mappers;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.Repositories;

internal sealed class TeacherProfileRepository(ITeacherProfileRemoteDataSource remoteDataSource) : ITeacherProfileRepository
{
    public async Task<Result<TeacherProfile>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.GetCurrentAsync(cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<TeacherProfile>> UpdateCurrentAsync(
        UpdateTeacherProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateCurrentAsync(
            TeacherProfileMapper.ToDto(command),
            cancellationToken);
        return MapResponse(result);
    }

    private static Result<TeacherProfile> MapResponse(Result<TeacherProfileResponseDto> result)
    {
        if (!result.IsSuccess)
        {
            return Result<TeacherProfile>.Failure(result.Error ?? new AppError(
                AppErrorKind.Unknown,
                "تعذر تفسير استجابة خادم ملف المعلم."));
        }

        if (result.Value?.TeacherProfile is null)
        {
            return Result<TeacherProfile>.Failure(new AppError(
                AppErrorKind.Unknown,
                "أعاد الخادم استجابة ملف معلم فارغة أو غير متوقعة."));
        }

        return TeacherProfileMapper.ToDomain(result.Value.TeacherProfile);
    }
}
