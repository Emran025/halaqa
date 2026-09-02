using Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Profile.Data.Mappers;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.Repositories;

internal sealed class StudentProfileRepository : IStudentProfileRepository
{

    private readonly IStudentProfileRemoteDataSource remoteDataSource;


    public StudentProfileRepository(

        IStudentProfileRemoteDataSource remoteDataSource

    )

    {

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<StudentProfile>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.GetCurrentAsync(cancellationToken);
        return MapResponse(result);
    }

    public async Task<Result<StudentProfile>> UpdateCurrentAsync(
        UpdateStudentProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.UpdateCurrentAsync(
            StudentProfileMapper.ToDto(command),
            cancellationToken);
        return MapResponse(result);
    }

    private static Result<StudentProfile> MapResponse(Result<StudentProfileResponseDto> result)
    {
        if (!result.IsSuccess)
        {
            return Result<StudentProfile>.Failure(result.Error ?? new AppError(
                AppErrorKind.Unknown,
                "تعذر تفسير استجابة خادم ملف الطالب."));
        }

        if (result.Value?.StudentProfile is null)
        {
            return Result<StudentProfile>.Failure(new AppError(
                AppErrorKind.Unknown,
                "أعاد الخادم استجابة ملف طالب فارغة أو غير متوقعة."));
        }

        return StudentProfileMapper.ToDomain(result.Value.StudentProfile);
    }
}
