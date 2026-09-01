using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Domain.UseCases;

public sealed class LogoutUseCase
{
    private readonly IAuthRepository _authRepository;

    public LogoutUseCase(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public Task<Result> ExecuteAsync(CancellationToken cancellationToken = default) =>
        _authRepository.LogoutAsync(cancellationToken);
}
