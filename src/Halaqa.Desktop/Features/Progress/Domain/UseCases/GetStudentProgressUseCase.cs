using Halaqa.Desktop.Features.Progress.Domain.Entities;
using Halaqa.Desktop.Features.Progress.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Progress.Domain.UseCases;

public sealed class GetStudentProgressUseCase
{
    private readonly IStudentProgressRepository repository;

    public GetStudentProgressUseCase(IStudentProgressRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<StudentProgress>> ExecuteAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default) =>
        studentId == Guid.Empty
            ? Task.FromResult(Result<StudentProgress>.Failure(new AppError(AppErrorKind.Validation, "معرّف الطالب غير صالح.")))
            : repository.GetAsync(studentId, taskType, cancellationToken);
}
