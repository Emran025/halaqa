using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Data.DataSources.Remote;

internal interface IRegistrationRequestRemoteDataSource
{
    Task<Result<RegistrationCollectionResponseDto>> ListMineAsync(
        string? state,
        int page,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationCollectionResponseDto>> ListForHalaqaAsync(
        Guid halaqaId,
        string? state,
        int page,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationResponseDto>> AcceptAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationResponseDto>> RejectAsync(
        Guid registrationId,
        DecisionNoteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationResponseDto>> RequestCompletionAsync(
        Guid registrationId,
        CompletionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);
}

internal sealed class RegistrationRequestRemoteDataSource(IApiClient apiClient) : IRegistrationRequestRemoteDataSource
{
    public Task<Result<RegistrationCollectionResponseDto>> ListMineAsync(
        string? state,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = $"registration-requests?page={page}";
        if (!string.IsNullOrWhiteSpace(state))
        {
            query += $"&state={Uri.EscapeDataString(state)}";
        }

        return apiClient.GetAsync<RegistrationCollectionResponseDto>(query, cancellationToken);
    }

    public Task<Result<RegistrationCollectionResponseDto>> ListForHalaqaAsync(
        Guid halaqaId,
        string? state,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = $"halaqas/{halaqaId}/registration-requests?page={page}";
        if (!string.IsNullOrWhiteSpace(state))
        {
            query += $"&state={Uri.EscapeDataString(state)}";
        }

        return apiClient.GetAsync<RegistrationCollectionResponseDto>(query, cancellationToken);
    }

    public Task<Result<RegistrationResponseDto>> AcceptAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default) =>
        apiClient.PostEmptyAsync<RegistrationResponseDto>(
            $"registration-requests/{registrationId}/accept",
            cancellationToken);

    public Task<Result<RegistrationResponseDto>> RejectAsync(
        Guid registrationId,
        DecisionNoteRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<DecisionNoteRequestDto, RegistrationResponseDto>(
            $"registration-requests/{registrationId}/reject",
            request,
            cancellationToken);

    public Task<Result<RegistrationResponseDto>> RequestCompletionAsync(
        Guid registrationId,
        CompletionRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<CompletionRequestDto, RegistrationResponseDto>(
            $"registration-requests/{registrationId}/request-completion",
            request,
            cancellationToken);

    public Task<Result> CancelAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default) =>
        apiClient.DeleteAsync($"registration-requests/{registrationId}", cancellationToken);
}
