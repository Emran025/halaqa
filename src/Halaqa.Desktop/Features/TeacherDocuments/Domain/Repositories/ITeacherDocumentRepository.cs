using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.TeacherDocuments.Domain.Repositories;

public interface ITeacherDocumentRepository
{
    Task<Result<TeacherDocumentPage>> ListAsync(
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<Result<TeacherDocument>> CreateAsync(
        CreateTeacherDocumentCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default);
}
