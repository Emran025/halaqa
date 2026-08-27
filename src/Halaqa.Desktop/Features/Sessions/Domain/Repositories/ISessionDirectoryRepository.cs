using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.Repositories;

public interface ISessionDirectoryRepository
{
    Task<Result<SessionPage>> ListAsync(SessionQuery query, CancellationToken cancellationToken = default);
}
