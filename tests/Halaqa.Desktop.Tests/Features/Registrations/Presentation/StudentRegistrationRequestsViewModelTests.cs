using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Presentation;

public sealed class StudentRegistrationRequestsViewModelTests
{
    [Fact]
    public async Task Cancel_ReloadsStudentRequestsFromRepositoryAfterSuccessfulWithdrawal()
    {
        var repository = new FakeRegistrationRepository();
        var viewModel = new StudentRegistrationRequestsViewModel(
            new ListMyRegistrationRequestsUseCase(repository),
            new CancelRegistrationRequestUseCase(repository));
        viewModel.Initialize();
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedRequest = Assert.Single(viewModel.Requests);

        await viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(repository.Request.Id, repository.CancelledRegistrationId);
        var reloaded = Assert.Single(viewModel.Requests);
        Assert.Equal(RegistrationState.Withdrawn, reloaded.State);
        Assert.Contains("تم سحب", viewModel.Message);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Cancel_IsDisabledForDecidedRequest()
    {
        var repository = new FakeRegistrationRepository { InitialState = RegistrationState.Accepted };
        var viewModel = new StudentRegistrationRequestsViewModel(
            new ListMyRegistrationRequestsUseCase(repository),
            new CancelRegistrationRequestUseCase(repository));
        viewModel.Initialize();
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedRequest = Assert.Single(viewModel.Requests);

        Assert.False(viewModel.CancelCommand.CanExecute(null));
    }

    private sealed class FakeRegistrationRepository : IRegistrationRequestRepository
    {
        public RegistrationState InitialState { get; init; } = RegistrationState.Pending;
        public RegistrationRequest Request { get; } = new(
            Guid.NewGuid(),
            new RegistrationApplicant(
                Guid.NewGuid(),
                "طالب اختبار",
                null,
                RegistrationState.Pending,
                DateTimeOffset.Parse("2026-08-25T09:00:00Z"),
                true),
            RegistrationState.Pending,
            "student_visible",
            "طلب موجّه",
            null,
            null,
            DateTimeOffset.Parse("2026-08-25T09:00:00Z"));

        public Guid? CancelledRegistrationId { get; private set; }

        public Task<Result<RegistrationRequestPage>> ListMineAsync(
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            var currentState = CancelledRegistrationId is null ? InitialState : RegistrationState.Withdrawn;
            return Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(
                new[] {Request with { State = currentState }},
                1,
                1,
                20,
                1)));
        }

        public Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(
            Guid halaqaId,
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(Array.Empty<RegistrationRequest>(), 1, 1, 20, 0)));

        public Task<Result<RegistrationRequestPage>> ListTeacherInboxAsync(
            RegistrationState? state = null,
            string? search = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(Array.Empty<RegistrationRequest>(), 1, 1, 20, 0)));

        public Task<Result<RegistrationRequest>> AcceptAsync(Guid registrationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(Request));

        public Task<Result<RegistrationRequest>> RejectAsync(
            RejectRegistrationRequestCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(Request));

        public Task<Result<RegistrationRequest>> RequestCompletionAsync(
            RequestRegistrationCompletionCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequest>.Success(Request));

        public Task<Result> CancelAsync(Guid registrationId, CancellationToken cancellationToken = default)
        {
            CancelledRegistrationId = registrationId;
            return Task.FromResult(Result.Success());
        }
    }
}
