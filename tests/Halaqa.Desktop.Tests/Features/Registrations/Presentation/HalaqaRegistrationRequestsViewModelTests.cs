using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Presentation;

public sealed class HalaqaRegistrationRequestsViewModelTests
{
    [Fact]
    public async Task Load_UsesSelectedHalaqaAndExposesOnlyPublicApplicantSummary()
    {
        var repository = new FakeRegistrationRepository();
        var halaqaId = Guid.NewGuid();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");

        await viewModel.LoadCommand.ExecuteAsync(null);

        var request = Assert.Single(viewModel.Requests);
        Assert.Equal(halaqaId, repository.ListHalaqaId);
        Assert.Equal("طالب اختبار", request.Applicant.DisplayName);
        Assert.True(request.Applicant.SensitiveFieldsHidden);
        Assert.Equal("حلقة الاختبار", viewModel.HalaqaName);
    }

    [Fact]
    public async Task RequestCompletion_NormalizesInputFieldsBeforeDelegation()
    {
        var repository = new FakeRegistrationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid(), "حلقة الاختبار");
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedRequest = Assert.Single(viewModel.Requests);
        viewModel.RequiredFields = " phone، country, phone ";
        viewModel.CompletionNote = "  يرجى استكمال البيانات  ";

        await viewModel.RequestCompletionCommand.ExecuteAsync(null);

        Assert.NotNull(repository.CompletionRequest);
        Assert.Equal(["phone", "country"], repository.CompletionRequest!.RequiredFields);
        Assert.Equal("يرجى استكمال البيانات", repository.CompletionRequest.Note);
        Assert.Equal(RegistrationState.CompletionRequested, viewModel.SelectedRequest?.State);
    }

    private static HalaqaRegistrationRequestsViewModel CreateViewModel(FakeRegistrationRepository repository) => new(
        new ListHalaqaRegistrationRequestsUseCase(repository),
        new AcceptRegistrationRequestUseCase(repository),
        new RejectRegistrationRequestUseCase(repository),
        new RequestRegistrationCompletionUseCase(repository));

    private sealed class FakeRegistrationRepository : IRegistrationRequestRepository
    {
        private readonly RegistrationRequest _request = CreateRequest();

        public Guid? ListHalaqaId { get; private set; }
        public RequestRegistrationCompletionCommand? CompletionRequest { get; private set; }

        public Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(
            Guid halaqaId,
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            ListHalaqaId = halaqaId;
            return Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage([_request], 1, 1, 20, 1)));
        }

        public Task<Result<RegistrationRequest>> AcceptAsync(Guid registrationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.Accepted }));

        public Task<Result<RegistrationRequest>> RejectAsync(
            RejectRegistrationRequestCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.Rejected }));

        public Task<Result<RegistrationRequest>> RequestCompletionAsync(
            RequestRegistrationCompletionCommand command,
            CancellationToken cancellationToken = default)
        {
            CompletionRequest = command;
            return Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.CompletionRequested }));
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
