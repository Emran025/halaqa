using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.Repositories;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.FollowUp.Presentation;

public sealed class ComprehensiveTrackingViewModelTests
{
    [Fact]
    public async Task LoadStudents_PopulatesStudentsAndCalculatesExpectedToday()
    {
        var halaqasRepo = new FakeHalaqasRepository();
        var membershipsRepo = new FakeMembershipsRepository();
        var listHalaqas = new ListHalaqasUseCase(halaqasRepo);
        var listMemberships = new ListHalaqaMembershipsUseCase(membershipsRepo);

        var viewModel = new ComprehensiveTrackingViewModel(listHalaqas, listMemberships);

        await viewModel.LoadStudentsAsync();

        Assert.NotEmpty(viewModel.DisplayedStudents);
        Assert.True(viewModel.ExpectedTodayCount > 0);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task MarkStudentCompleted_UpdatesRecitationStatusAndCounts()
    {
        var halaqasRepo = new FakeHalaqasRepository();
        var membershipsRepo = new FakeMembershipsRepository();
        var listHalaqas = new ListHalaqasUseCase(halaqasRepo);
        var listMemberships = new ListHalaqaMembershipsUseCase(membershipsRepo);

        var viewModel = new ComprehensiveTrackingViewModel(listHalaqas, listMemberships);
        await viewModel.LoadStudentsAsync();

        var firstStudent = viewModel.DisplayedStudents[0];
        var initialExpected = viewModel.ExpectedTodayCount;
        var initialCompleted = viewModel.CompletedTodayCount;

        viewModel.MarkStudentCompleted(firstStudent.StudentId);

        Assert.Equal(initialCompleted + 1, viewModel.CompletedTodayCount);
    }

    [Fact]
    public async Task FilterBySearchText_FiltersMatchingStudents()
    {
        var halaqasRepo = new FakeHalaqasRepository();
        var membershipsRepo = new FakeMembershipsRepository();
        var listHalaqas = new ListHalaqasUseCase(halaqasRepo);
        var listMemberships = new ListHalaqaMembershipsUseCase(membershipsRepo);

        var viewModel = new ComprehensiveTrackingViewModel(listHalaqas, listMemberships);
        await viewModel.LoadStudentsAsync();

        var targetStudent = viewModel.DisplayedStudents[0];
        viewModel.SearchText = targetStudent.StudentName;

        Assert.Contains(viewModel.DisplayedStudents, s => s.StudentName == targetStudent.StudentName);
    }

    [Fact]
    public async Task StartSardRecitation_TriggersRecitationRequestedWithSard()
    {
        var halaqasRepo = new FakeHalaqasRepository();
        var membershipsRepo = new FakeMembershipsRepository();
        var listHalaqas = new ListHalaqasUseCase(halaqasRepo);
        var listMemberships = new ListHalaqaMembershipsUseCase(membershipsRepo);

        var viewModel = new ComprehensiveTrackingViewModel(listHalaqas, listMemberships);
        await viewModel.LoadStudentsAsync();

        var student = viewModel.DisplayedStudents[0];
        string? receivedTaskType = null;
        viewModel.RecitationRequested += (_, args) => receivedTaskType = args.TaskType;

        viewModel.StartSardRecitationCommand.Execute(student);

        Assert.Equal("سرد", receivedTaskType);
    }

    private sealed class FakeHalaqasRepository : IHalaqaRepository
    {
        public Task<Result<HalaqaPage>> ListAsync(int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<HalaqaPage>.Success(new HalaqaPage(Array.Empty<HalaqaItem>(), 1, 1, 10, 0)));

        public Task<Result<HalaqaItem>> CreateAsync(CreateHalaqaCommand command, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result<HalaqaItem>> UpdateAsync(UpdateHalaqaCommand command, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result<HalaqaItem>> ActivateAsync(Guid halaqaId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result<HalaqaItem>> DeactivateAsync(Guid halaqaId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeMembershipsRepository : IHalaqaMembershipRepository
    {
        public Task<Result<MembershipPage>> ListAsync(Guid halaqaId, string? status = null, int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<MembershipPage>.Success(new MembershipPage(Array.Empty<HalaqaMembership>(), 1, 1, 10, 0)));

        public Task<Result<HalaqaMembership>> AssignAsync(AssignStudentToHalaqaCommand command, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result<HalaqaMembership>> UpdateAsync(UpdateHalaqaMembershipCommand command, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result> RemoveAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}