using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Memberships.Presentation;

public sealed class HalaqaMembershipsViewModelTests
{
    [Fact]
    public async Task Load_PopulatesMembershipsAndUpdatesPagination()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");

        await viewModel.LoadCommand.ExecuteAsync(null);

        var membership = Assert.Single(viewModel.Memberships);
        Assert.Equal(halaqaId, repository.ListHalaqaId);
        Assert.Equal("طالب اختبار", membership.Student.Name);
        Assert.Equal(1, viewModel.Total);
        Assert.False(viewModel.HasNoMemberships);
    }

    [Fact]
    public async Task SearchText_FiltersMembershipsLocally()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var student1 = new MembershipStudent(Guid.NewGuid(), "أحمد محمد", "ahmed@halaqa.local", "0501111111", "active", null, null);
        var student2 = new MembershipStudent(Guid.NewGuid(), "سالم خالد", "salem@halaqa.local", "0502222222", "active", null, null);
        repository.SeedMemberships = new List<HalaqaMembership>
        {
            new(Guid.NewGuid(), halaqaId, student1, MembershipStatus.Active, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), halaqaId, student2, MembershipStatus.Active, DateTimeOffset.UtcNow)
        };

        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");
        await viewModel.LoadCommand.ExecuteAsync(null);
        Assert.Equal(2, viewModel.Memberships.Count);

        // Search by name
        viewModel.SearchText = "أحمد";
        Assert.Single(viewModel.Memberships);
        Assert.Equal("أحمد محمد", viewModel.Memberships[0].Student.Name);

        // Search by email
        viewModel.SearchText = "salem@";
        Assert.Single(viewModel.Memberships);
        Assert.Equal("سالم خالد", viewModel.Memberships[0].Student.Name);

        // Search clear
        viewModel.SearchText = string.Empty;
        Assert.Equal(2, viewModel.Memberships.Count);
    }

    [Fact]
    public void OpenAssignDialog_ResetsFieldsAndSetsAssignMode()
    {
        var repository = new FakeMembershipRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid(), "حلقة الاختبار");

        viewModel.OpenAssignDialogCommand.Execute(null);

        Assert.True(viewModel.IsDialogOpen);
        Assert.True(viewModel.IsAssignMode);
        Assert.Null(viewModel.SelectedMembership);
        Assert.Equal(string.Empty, viewModel.StudentId);
        Assert.Equal("إسناد طالب جديد إلى الحلقة", viewModel.DialogTitle);
    }

    [Fact]
    public async Task Assign_ValidGuid_CallsUseCaseAndAddsMembership()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");

        viewModel.OpenAssignDialogCommand.Execute(null);
        viewModel.StudentId = studentId.ToString();

        await viewModel.AssignCommand.ExecuteAsync(null);

        Assert.NotNull(repository.Assignment);
        Assert.Equal(halaqaId, repository.Assignment!.HalaqaId);
        Assert.Equal(studentId, repository.Assignment!.StudentId);
        Assert.False(viewModel.IsDialogOpen);
        Assert.Single(viewModel.Memberships);
    }

    [Fact]
    public void OpenManageDialog_SetsSelectedMembershipAndManageMode()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var student = new MembershipStudent(Guid.NewGuid(), "عمر علي", "omar@halaqa.local", null, "active", null, null);
        var membership = new HalaqaMembership(Guid.NewGuid(), halaqaId, student, MembershipStatus.Active, DateTimeOffset.UtcNow);

        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");

        viewModel.OpenManageDialogCommand.Execute(membership);

        Assert.True(viewModel.IsDialogOpen);
        Assert.False(viewModel.IsAssignMode);
        Assert.Same(membership, viewModel.SelectedMembership);
        Assert.Equal("active", viewModel.SelectedStatus);
        Assert.Equal("إدارة عضوية: عمر علي", viewModel.DialogTitle);
    }

    [Fact]
    public async Task UpdateStatus_CallsUseCaseAndUpdatesMembership()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var student = new MembershipStudent(Guid.NewGuid(), "عمر علي", "omar@halaqa.local", null, "active", null, null);
        var membership = new HalaqaMembership(Guid.NewGuid(), halaqaId, student, MembershipStatus.Active, DateTimeOffset.UtcNow);
        repository.SeedMemberships = new List<HalaqaMembership> { membership };

        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.OpenManageDialogCommand.Execute(viewModel.Memberships[0]);
        viewModel.SelectedStatus = "inactive";
        viewModel.Reason = "طلب إجازة مؤقتة";

        await viewModel.UpdateStatusCommand.ExecuteAsync(null);

        Assert.NotNull(repository.LastUpdate);
        Assert.Equal(MembershipStatus.Inactive, repository.LastUpdate!.Status);
        Assert.Equal("طلب إجازة مؤقتة", repository.LastUpdate.Reason);
        Assert.Equal(MembershipStatus.Inactive, viewModel.SelectedMembership?.Status);
    }

    [Fact]
    public async Task Remove_CallsUseCaseAndRemovesMembership()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var student = new MembershipStudent(Guid.NewGuid(), "عمر علي", "omar@halaqa.local", null, "active", null, null);
        var membership = new HalaqaMembership(Guid.NewGuid(), halaqaId, student, MembershipStatus.Active, DateTimeOffset.UtcNow);
        repository.SeedMemberships = new List<HalaqaMembership> { membership };

        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(halaqaId, "حلقة الاختبار");
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.OpenManageDialogCommand.Execute(viewModel.Memberships[0]);
        await viewModel.RemoveCommand.ExecuteAsync(null);

        Assert.NotNull(repository.RemovedMembershipId);
        Assert.Equal(membership.Id, repository.RemovedMembershipId);
        Assert.Empty(viewModel.Memberships);
        Assert.False(viewModel.IsDialogOpen);
    }

    private static HalaqaMembershipsViewModel CreateViewModel(IHalaqaMembershipRepository repository) =>
        new(
            new ListHalaqaMembershipsUseCase(repository),
            new AssignStudentToHalaqaUseCase(repository),
            new UpdateHalaqaMembershipUseCase(repository),
            new RemoveHalaqaMembershipUseCase(repository));

    private sealed class FakeMembershipRepository : IHalaqaMembershipRepository
    {
        public Guid? ListHalaqaId { get; private set; }
        public AssignStudentToHalaqaCommand? Assignment { get; private set; }
        public UpdateHalaqaMembershipCommand? LastUpdate { get; private set; }
        public Guid? RemovedMembershipId { get; private set; }
        public List<HalaqaMembership> SeedMemberships { get; set; } = new();

        public Task<Result<MembershipPage>> ListAsync(Guid halaqaId, string? status = null, int page = 1, int perPage = 30, CancellationToken cancellationToken = default)
        {
            ListHalaqaId = halaqaId;
            var list = SeedMemberships.Count > 0
                ? SeedMemberships
                : new List<HalaqaMembership>
                {
                    new(
                        Guid.NewGuid(),
                        halaqaId,
                        new MembershipStudent(Guid.NewGuid(), "طالب اختبار", "test@halaqa.local", "0500000000", "active", null, null),
                        MembershipStatus.Active,
                        DateTimeOffset.UtcNow)
                };

            return Task.FromResult(Result<MembershipPage>.Success(new MembershipPage(list, page, 1, perPage, list.Count)));
        }

        public Task<Result<HalaqaMembership>> AssignAsync(AssignStudentToHalaqaCommand command, CancellationToken cancellationToken = default)
        {
            Assignment = command;
            var created = new HalaqaMembership(
                Guid.NewGuid(),
                command.HalaqaId,
                new MembershipStudent(command.StudentId, "طالب مسند جديد", "assigned@halaqa.local", null, "active", null, null),
                MembershipStatus.Active,
                DateTimeOffset.UtcNow);
            return Task.FromResult(Result<HalaqaMembership>.Success(created));
        }

        public Task<Result<HalaqaMembership>> UpdateAsync(UpdateHalaqaMembershipCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdate = command;
            var updated = new HalaqaMembership(
                command.MembershipId,
                command.HalaqaId,
                new MembershipStudent(Guid.NewGuid(), "طالب محدث", "updated@halaqa.local", null, "active", null, null),
                command.Status,
                DateTimeOffset.UtcNow);
            return Task.FromResult(Result<HalaqaMembership>.Success(updated));
        }

        public Task<Result> RemoveAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default)
        {
            RemovedMembershipId = membershipId;
            return Task.FromResult(Result.Success());
        }
    }
}
