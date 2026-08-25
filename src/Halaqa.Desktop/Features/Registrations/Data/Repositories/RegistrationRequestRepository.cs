using Halaqa.Desktop.Features.Registrations.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Registrations.Data.Mappers;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Data.Repositories;

internal sealed class RegistrationRequestRepository(
    IRegistrationRequestRemoteDataSource remoteDataSource) : IRegistrationRequestRepository
{
    public async Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(
        Guid halaqaId,
        RegistrationState? state = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.ListForHalaqaAsync(
            halaqaId,
            state is null ? null : RegistrationRequestMapper.ToContractValue(state.Value),
            page,
            cancellationToken);

        return response.IsSuccess && response.Value is not null
            ? RegistrationRequestMapper.ToDomain(response.Value)
            : Result<RegistrationRequestPage>.Failure(response.Error!);
    }

    public async Task<Result<RegistrationRequest>> AcceptAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.AcceptAsync(registrationId, cancellationToken);
        return ToDomain(response);
    }

    public async Task<Result<RegistrationRequest>> RejectAsync(
        RejectRegistrationRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.RejectAsync(
            command.RegistrationId,
            RegistrationRequestMapper.ToDto(command),
            cancellationToken);
        return ToDomain(response);
    }

    public async Task<Result<RegistrationRequest>> RequestCompletionAsync(
        RequestRegistrationCompletionCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.RequestCompletionAsync(
            command.RegistrationId,
            RegistrationRequestMapper.ToDto(command),
            cancellationToken);
        return ToDomain(response);
    }

    private static Result<RegistrationRequest> ToDomain(Result<Data.Models.RegistrationResponseDto> response) =>
        response.IsSuccess && response.Value?.RegistrationRequest is not null
            ? RegistrationRequestMapper.ToDomain(response.Value.RegistrationRequest)
            : Result<RegistrationRequest>.Failure(response.Error!);
}
