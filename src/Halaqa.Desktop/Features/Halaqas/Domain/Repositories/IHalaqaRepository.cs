using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Halaqas.Domain.Repositories;

public interface IHalaqaRepository
{
    Task<Result<HalaqaPage>> ListAsync(
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<Result<Halaqa>> CreateAsync(
        CreateHalaqaCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Halaqa>> UpdateAsync(
        UpdateHalaqaCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Halaqa>> ActivateAsync(Guid halaqaId, CancellationToken cancellationToken = default);

    Task<Result<Halaqa>> DeactivateAsync(Guid halaqaId, CancellationToken cancellationToken = default);
}
