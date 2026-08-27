using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.Repositories;

public interface ISessionTaskDirectoryRepository
{
    Task<Result<SessionTaskPage>> ListAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
