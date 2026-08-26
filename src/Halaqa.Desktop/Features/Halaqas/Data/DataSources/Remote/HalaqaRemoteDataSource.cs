using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Halaqas.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Halaqas.Data.DataSources.Remote;

internal interface IHalaqaRemoteDataSource
{
    Task<Result<HalaqaCollectionResponseDto>> ListAsync(int page, CancellationToken cancellationToken = default);
    Task<Result<HalaqaResponseDto>> CreateAsync(CreateHalaqaRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<HalaqaResponseDto>> UpdateAsync(Guid id, UpdateHalaqaRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<HalaqaResponseDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<HalaqaResponseDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

internal sealed class HalaqaRemoteDataSource : IHalaqaRemoteDataSource
{

    private readonly IApiClient apiClient;


    public HalaqaRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<HalaqaCollectionResponseDto>> ListAsync(int page, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<HalaqaCollectionResponseDto>($"halaqas?page={page}", cancellationToken);

    public Task<Result<HalaqaResponseDto>> CreateAsync(CreateHalaqaRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<CreateHalaqaRequestDto, HalaqaResponseDto>("halaqas", request, cancellationToken);

    public Task<Result<HalaqaResponseDto>> UpdateAsync(Guid id, UpdateHalaqaRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PatchAsync<UpdateHalaqaRequestDto, HalaqaResponseDto>($"halaqas/{id}", request, cancellationToken);

    public Task<Result<HalaqaResponseDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<object, HalaqaResponseDto>($"halaqas/{id}/activate", new { }, cancellationToken);

    public Task<Result<HalaqaResponseDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<object, HalaqaResponseDto>($"halaqas/{id}/deactivate", new { }, cancellationToken);
}
