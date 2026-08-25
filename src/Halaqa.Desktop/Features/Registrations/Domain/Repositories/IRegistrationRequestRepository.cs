using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Domain.Repositories;

public interface IRegistrationRequestRepository
{
    Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(
        Guid halaqaId,
        RegistrationState? state = null,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationRequest>> AcceptAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationRequest>> RejectAsync(
        RejectRegistrationRequestCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationRequest>> RequestCompletionAsync(
        RequestRegistrationCompletionCommand command,
        CancellationToken cancellationToken = default);
}
