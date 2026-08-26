using Halaqa.Desktop.Features.TeacherDocuments.Data.DataSources.Remote;
using Halaqa.Desktop.Features.TeacherDocuments.Data.Mappers;
using Halaqa.Desktop.Features.TeacherDocuments.Data.Models;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.TeacherDocuments.Data.Repositories;

internal sealed class TeacherDocumentRepository : ITeacherDocumentRepository
{

    private readonly ITeacherDocumentRemoteDataSource remoteDataSource;


    public TeacherDocumentRepository(

        ITeacherDocumentRemoteDataSource remoteDataSource

    )

    {

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<TeacherDocumentPage>> ListAsync(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.ListAsync(page, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<TeacherDocumentPage>.Failure(result.Error ?? UnknownError());
        }
        if (result.Value is null)
        {
            return Result<TeacherDocumentPage>.Failure(UnknownError());
        }

        return TeacherDocumentMapper.ToDomain(result.Value);
    }

    public async Task<Result<TeacherDocument>> CreateAsync(
        CreateTeacherDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.CreateAsync(
            TeacherDocumentMapper.ToUploadDto(command),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<TeacherDocument>.Failure(result.Error ?? UnknownError());
        }
        if (result.Value?.TeacherDocument is null)
        {
            return Result<TeacherDocument>.Failure(UnknownError());
        }

        return TeacherDocumentMapper.ToDomain(result.Value.TeacherDocument);
    }

    public Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default) =>
        remoteDataSource.DeleteAsync(documentId, cancellationToken);

    private static AppError UnknownError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم استجابة وثائق معلم فارغة أو غير متوقعة.");
}
