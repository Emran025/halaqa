using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Presentation;

public sealed class TeacherApplicationInboxViewModelTests
{
    [Fact]
    public async Task Load_UsesTeacherInboxWithStateAndSearch()
    {
        var repository = new FakeRegistrationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize();
        viewModel.SearchText = "  طالب  ";

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(RegistrationState.Pending, repository.InboxState);
        Assert.Equal("طالب", repository.InboxSearch);
        Assert.Single(viewModel.Requests);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Accept_UpdatesSelectedRequestFromOfficialResponse()
    {
        var repository = new FakeRegistrationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize();
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedRequest = Assert.Single(viewModel.Requests);

        await viewModel.AcceptCommand.ExecuteAsync(null);

        Assert.Equal(RegistrationState.Accepted, viewModel.SelectedRequest!.State);
        Assert.Contains("قبول", viewModel.Message);
    }

    private static TeacherApplicationInboxViewModel CreateViewModel(IRegistrationRequestRepository repository) => new(
        new ListTeacherApplicationInboxUseCase(repository),
        new AcceptRegistrationRequestUseCase(repository),
        new RejectRegistrationRequestUseCase(repository),
        new RequestRegistrationCompletionUseCase(repository));

    private sealed class FakeRegistrationRepository : IRegistrationRequestRepository
    {
        private readonly RegistrationRequest _request = CreateRequest();
        public RegistrationState? InboxState { get; private set; }
        public string? InboxSearch { get; private set; }

        public Task<Result<RegistrationRequestPage>> ListTeacherInboxAsync(RegistrationState? state = null, string? search = null, int page = 1, CancellationToken cancellationToken = default)
        {
            InboxState = state;
            InboxSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            return Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(new[] { _request }, 1, 1, 20, 1)));
        }

        public Task<Result<RegistrationRequestPage>> ListMineAsync(RegistrationState? state = null, int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(Array.Empty<RegistrationRequest>(), 1, 1, 20, 0)));
        public Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(Guid halaqaId, RegistrationState? state = null, int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(Array.Empty<RegistrationRequest>(), 1, 1, 20, 0)));
        public Task<Result<RegistrationRequest>> AcceptAsync(Guid registrationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.Accepted }));
        public Task<Result<RegistrationRequest>> RejectAsync(RejectRegistrationRequestCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.Rejected }));
        public Task<Result<RegistrationRequest>> RequestCompletionAsync(RequestRegistrationCompletionCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.CompletionRequested }));
        public Task<Result> CancelAsync(Guid registrationId, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success());

        private static RegistrationRequest CreateRequest() => new(
            Guid.NewGuid(),
            new RegistrationApplicant(Guid.NewGuid(), "طالب اختبار", null, RegistrationState.Pending, DateTimeOffset.Parse("2026-08-26T09:00:00Z"), true),
            RegistrationState.Pending,
            "public_summary",
            "طلب عام",
            null,
            null,
            DateTimeOffset.Parse("2026-08-26T09:00:00Z"));
    }
}
