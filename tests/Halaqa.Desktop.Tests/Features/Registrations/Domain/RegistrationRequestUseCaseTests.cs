using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Domain;

public sealed class RegistrationRequestUseCaseTests
{
    [Fact]
    public async Task RequestCompletion_RejectsEmptyRequiredFieldsBeforeCallingRepository()
    {
        var repository = new FakeRegistrationRepository();
        var command = new RequestRegistrationCompletionCommand(Guid.NewGuid(), [], "يرجى استكمال البيانات");

        var result = await new RequestRegistrationCompletionUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.CompletionRequest);
    }

    [Fact]
    public async Task Reject_RejectsNoteLongerThanContractLimit()
    {
        var repository = new FakeRegistrationRepository();
        var command = new RejectRegistrationRequestCommand(Guid.NewGuid(), new string('x', 1001));

        var result = await new RejectRegistrationRequestUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.Rejection);
    }

    [Fact]
    public async Task ListMine_RejectsInvalidPageBeforeCallingRepository()
    {
        var repository = new FakeRegistrationRepository();

        var result = await new ListMyRegistrationRequestsUseCase(repository).ExecuteAsync(page: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.MineListPage);
    }

    [Fact]
    public async Task Cancel_ForwardsValidRegistrationIdentifierToRepository()
    {
        var repository = new FakeRegistrationRepository();
        var registrationId = Guid.NewGuid();

        var result = await new CancelRegistrationRequestUseCase(repository).ExecuteAsync(registrationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(registrationId, repository.CancelledRegistrationId);
    }

    [Fact]
    public async Task Accept_ForwardsValidRegistrationIdentifierToRepository()
    {
        var repository = new FakeRegistrationRepository();
        var registrationId = Guid.NewGuid();

        var result = await new AcceptRegistrationRequestUseCase(repository).ExecuteAsync(registrationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(registrationId, repository.AcceptedRegistrationId);
    }

    private sealed class FakeRegistrationRepository : IRegistrationRequestRepository
    {
        public Guid? AcceptedRegistrationId { get; private set; }
        public int? MineListPage { get; private set; }
        public Guid? CancelledRegistrationId { get; private set; }
        public RejectRegistrationRequestCommand? Rejection { get; private set; }
        public RequestRegistrationCompletionCommand? CompletionRequest { get; private set; }

        public Task<Result<RegistrationRequestPage>> ListMineAsync(
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            MineListPage = page;
            return Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage([], 1, 1, 20, 0)));
        }

        public Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(
            Guid halaqaId,
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage([], 1, 1, 20, 0)));

        public Task<Result<RegistrationRequestPage>> ListTeacherInboxAsync(
            RegistrationState? state = null,
            string? search = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage([], 1, 1, 20, 0)));

        public Task<Result<RegistrationRequest>> AcceptAsync(Guid registrationId, CancellationToken cancellationToken = default)
        {
            AcceptedRegistrationId = registrationId;
            return Task.FromResult(Result<RegistrationRequest>.Success(CreateRequest()));
        }

        public Task<Result<RegistrationRequest>> RejectAsync(
            RejectRegistrationRequestCommand command,
            CancellationToken cancellationToken = default)
        {
            Rejection = command;
            return Task.FromResult(Result<RegistrationRequest>.Success(CreateRequest()));
        }

        public Task<Result<RegistrationRequest>> RequestCompletionAsync(
            RequestRegistrationCompletionCommand command,
            CancellationToken cancellationToken = default)
        {
            CompletionRequest = command;
            return Task.FromResult(Result<RegistrationRequest>.Success(CreateRequest()));
        }

        public Task<Result> CancelAsync(Guid registrationId, CancellationToken cancellationToken = default)
        {
            CancelledRegistrationId = registrationId;
            return Task.FromResult(Result.Success());
        }

        private static RegistrationRequest CreateRequest() => new(
            Guid.NewGuid(),
            new RegistrationApplicant(
                Guid.NewGuid(),
                "طالب اختبار",
                null,
                RegistrationState.Pending,
                DateTimeOffset.Parse("2026-08-25T09:00:00Z"),
                true),
            RegistrationState.Pending,
            "public_summary",
            null,
            null,
            null,
            DateTimeOffset.Parse("2026-08-25T09:00:00Z"));
    }
}
