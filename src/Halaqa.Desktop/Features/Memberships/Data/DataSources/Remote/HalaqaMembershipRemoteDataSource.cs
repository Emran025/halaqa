using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Memberships.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Memberships.Data.DataSources.Remote;

internal interface IHalaqaMembershipRemoteDataSource
{
    Task<Result<MembershipCollectionResponseDto>> ListAsync(
        Guid halaqaId,
        string? status,
        int page,
        CancellationToken cancellationToken = default);

    Task<Result<MembershipResponseDto>> AssignAsync(
        Guid halaqaId,
        AssignStudentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<MembershipResponseDto>> UpdateAsync(
        Guid halaqaId,
        Guid membershipId,
        UpdateMembershipRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(
        Guid halaqaId,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}

internal sealed class HalaqaMembershipRemoteDataSource : IHalaqaMembershipRemoteDataSource
{

    private readonly IApiClient apiClient;


    public HalaqaMembershipRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<MembershipCollectionResponseDto>> ListAsync(
        Guid halaqaId,
        string? status,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = $"halaqas/{halaqaId}/memberships?page={page}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            query += $"&status={Uri.EscapeDataString(status)}";
        }

        return apiClient.GetAsync<MembershipCollectionResponseDto>(query, cancellationToken);
    }

    public Task<Result<MembershipResponseDto>> AssignAsync(
        Guid halaqaId,
        AssignStudentRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<AssignStudentRequestDto, MembershipResponseDto>(
            $"halaqas/{halaqaId}/students",
            request,
            cancellationToken);

    public Task<Result<MembershipResponseDto>> UpdateAsync(
        Guid halaqaId,
        Guid membershipId,
        UpdateMembershipRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PatchAsync<UpdateMembershipRequestDto, MembershipResponseDto>(
            $"halaqas/{halaqaId}/memberships/{membershipId}",
            request,
            cancellationToken);

    public Task<Result> RemoveAsync(
        Guid halaqaId,
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        apiClient.DeleteAsync($"halaqas/{halaqaId}/memberships/{membershipId}", cancellationToken);
}
