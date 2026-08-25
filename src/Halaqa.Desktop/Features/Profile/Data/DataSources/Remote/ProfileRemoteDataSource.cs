using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;

internal interface IProfileRemoteDataSource
{
    Task<Result<UserProfileResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<UserProfileResponseDto>> UpdateCurrentAsync(UpdateUserProfileRequestDto request, CancellationToken cancellationToken = default);
}

internal sealed class ProfileRemoteDataSource(IApiClient apiClient) : IProfileRemoteDataSource
{
    public Task<Result<UserProfileResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<UserProfileResponseDto>("me", cancellationToken);

    public Task<Result<UserProfileResponseDto>> UpdateCurrentAsync(UpdateUserProfileRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PatchAsync<UpdateUserProfileRequestDto, UserProfileResponseDto>("me", request, cancellationToken);
}
