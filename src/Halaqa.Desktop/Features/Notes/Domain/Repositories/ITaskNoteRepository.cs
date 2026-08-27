using Halaqa.Desktop.Features.Notes.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notes.Domain.Repositories;

public interface ITaskNoteRepository
{
    Task<Result<TaskNotePage>> ListAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default);

    Task<Result<TaskNote>> CreateAsync(CreateTaskNoteCommand command, CancellationToken cancellationToken = default);

    Task<Result<TaskNote>> UpdateAsync(UpdateTaskNoteCommand command, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(DeleteTaskNoteCommand command, CancellationToken cancellationToken = default);
}
