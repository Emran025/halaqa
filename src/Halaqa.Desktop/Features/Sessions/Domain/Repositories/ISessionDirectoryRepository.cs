using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.Repositories;

public interface ISessionDirectoryRepository
{
    Task<Result<SessionListItem>> CreateAsync(CreateLiveSessionCommand command, CancellationToken cancellationToken = default);
    Task<Result<SessionListItem>> AcceptAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<SessionListItem>> RejectAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<SessionPage>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default);
}
