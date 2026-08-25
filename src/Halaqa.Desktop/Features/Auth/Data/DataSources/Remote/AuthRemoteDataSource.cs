using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Auth.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Data.DataSources.Remote;

internal interface IAuthRemoteDataSource
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
}

internal sealed class AuthRemoteDataSource(IApiClient apiClient) : IAuthRemoteDataSource
{
    public Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<LoginRequestDto, AuthResponseDto>("auth/login", request, cancellationToken);

    public Task<Result> LogoutAsync(CancellationToken cancellationToken = default) =>
        apiClient.PostAsync("auth/logout", new { }, cancellationToken);
}
