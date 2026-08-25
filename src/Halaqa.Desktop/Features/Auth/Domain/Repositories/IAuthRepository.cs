using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Domain.Repositories;

public interface IAuthRepository
{
    Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
}
