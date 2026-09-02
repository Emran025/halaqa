using Halaqa.Desktop.Features.Progress.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Progress.Data.Mappers;
using Halaqa.Desktop.Features.Progress.Domain.Entities;
using Halaqa.Desktop.Features.Progress.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Progress.Data.Repositories;

internal sealed class StudentProgressRepository : IStudentProgressRepository
{
    private readonly IStudentProgressRemoteDataSource remoteDataSource;

    public StudentProgressRepository(IStudentProgressRemoteDataSource remoteDataSource)
    {
        this.remoteDataSource = remoteDataSource;
    }

    public async Task<Result<StudentProgress>> GetAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.GetAsync(studentId, taskType, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Result<StudentProgress>.Success(StudentProgressMapper.ToDomain(result.Value.Progress))
            : Result<StudentProgress>.Failure(result.Error ?? new AppError(AppErrorKind.Unknown, "تعذر تحميل تقدم الطالب."));
    }
}
