using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
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
        Assert.Equal(new[] { "phone", "country" }, repository.CompletionRequest!.RequiredFields);
        Assert.Equal("يرجى استكمال البيانات", repository.CompletionRequest.Note);
        Assert.Equal(RegistrationState.CompletionRequested, viewModel.SelectedRequest?.State);
    }

    [Fact]
    public async Task Accept_PassesSelectedHalaqaIdToUseCase()
    {
        var repository = new FakeRegistrationRepository();
        var viewModel = CreateViewModel(repository);
        var halaqaId = Guid.NewGuid();
        viewModel.Initialize(halaqaId, "حلقة الاختبار");
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedRequest = Assert.Single(viewModel.Requests);

        await viewModel.AcceptCommand.ExecuteAsync(null);

        Assert.Equal(halaqaId, repository.AcceptedTargetHalaqaId);
        Assert.Equal(RegistrationState.Accepted, viewModel.SelectedRequest?.State);
        Assert.True(viewModel.IsAcceptedState);
        Assert.False(viewModel.IsPendingState);
    }

    [Fact]
    public async Task AssignToHalaqa_DelegatesToMembershipUseCase()
    {
        var repository = new FakeRegistrationRepository();
        var membershipRepository = new FakeMembershipRepository();
        var viewModel = CreateViewModel(repository, membershipRepository);
        var halaqaId = Guid.NewGuid();
        viewModel.Initialize(halaqaId, "حلقة الاختبار");
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedRequest = Assert.Single(viewModel.Requests);

        await viewModel.AcceptCommand.ExecuteAsync(null);
        Assert.True(viewModel.AssignToHalaqaCommand.CanExecute(null));

        await viewModel.AssignToHalaqaCommand.ExecuteAsync(null);

        Assert.NotNull(membershipRepository.Assignment);
        Assert.Equal(halaqaId, membershipRepository.Assignment!.HalaqaId);
        Assert.Equal(viewModel.SelectedRequest!.Applicant.Id, membershipRepository.Assignment!.StudentId);
    }

    private static HalaqaRegistrationRequestsViewModel CreateViewModel(
        FakeRegistrationRepository repository,
        FakeMembershipRepository? membershipRepository = null) => new(
        new ListHalaqaRegistrationRequestsUseCase(repository),
        new AcceptRegistrationRequestUseCase(repository),
        new RejectRegistrationRequestUseCase(repository),
        new RequestRegistrationCompletionUseCase(repository),
        new AssignStudentToHalaqaUseCase(membershipRepository ?? new FakeMembershipRepository()));

    private sealed class FakeMembershipRepository : IHalaqaMembershipRepository
    {
        public AssignStudentToHalaqaCommand? Assignment { get; private set; }

        public Task<Result<MembershipPage>> ListAsync(Guid halaqaId, string? status = null, int page = 1, int perPage = 30, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<MembershipPage>.Success(new MembershipPage(Array.Empty<HalaqaMembership>(), 1, 1, 20, 0)));

        public Task<Result<HalaqaMembership>> AssignAsync(AssignStudentToHalaqaCommand command, CancellationToken cancellationToken = default)
        {
            Assignment = command;
            return Task.FromResult(Result<HalaqaMembership>.Success(new HalaqaMembership(
                Guid.NewGuid(),
                command.HalaqaId,
                new MembershipStudent(command.StudentId, "طالب", "student@example.test", null, "active", null, null),
                MembershipStatus.Active,
                DateTimeOffset.UtcNow)));
        }

        public Task<Result<HalaqaMembership>> UpdateAsync(UpdateHalaqaMembershipCommand command, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result> RemoveAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeRegistrationRepository : IRegistrationRequestRepository
    {
        private readonly RegistrationRequest _request = CreateRequest();

        public Guid? ListHalaqaId { get; private set; }
        public Guid? AcceptedTargetHalaqaId { get; private set; }
        public RequestRegistrationCompletionCommand? CompletionRequest { get; private set; }

        public Task<Result<RegistrationRequestPage>> ListMineAsync(
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(new[] { _request }, 1, 1, 20, 1)));

        public Task<Result<RegistrationRequestPage>> ListForHalaqaAsync(
            Guid halaqaId,
            RegistrationState? state = null,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            ListHalaqaId = halaqaId;
            return Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(new[] { _request }, 1, 1, 20, 1)));
        }

        public Task<Result<RegistrationRequestPage>> ListTeacherInboxAsync(
            RegistrationState? state = null,
            string? search = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(new[] { _request }, 1, 1, 20, 1)));

        public Task<Result<RegistrationRequest>> AcceptAsync(
            Guid registrationId,
            Guid? targetHalaqaId = null,
            CancellationToken cancellationToken = default)
        {
            AcceptedTargetHalaqaId = targetHalaqaId;
            return Task.FromResult(Result<RegistrationRequest>.Success(_request with { State = RegistrationState.Accepted }));
        }

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

        public Task<Result> CancelAsync(Guid registrationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

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
