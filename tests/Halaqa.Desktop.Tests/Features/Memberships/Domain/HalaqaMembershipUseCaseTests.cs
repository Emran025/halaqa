using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Memberships.Domain;

public sealed class HalaqaMembershipUseCaseTests
{
    [Fact]
    public async Task Assign_RejectsEmptyStudentIdentifierBeforeCallingRepository()
    {
        var repository = new FakeMembershipRepository();
        var command = new AssignStudentToHalaqaCommand(Guid.NewGuid(), Guid.Empty);

        var result = await new AssignStudentToHalaqaUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.Assignment);
    }

    [Fact]
    public async Task Update_RejectsReasonLongerThanContractLimit()
    {
        var repository = new FakeMembershipRepository();
        var command = new UpdateHalaqaMembershipCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            MembershipStatus.Inactive,
            new string('x', 501));

        var result = await new UpdateHalaqaMembershipUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.Update);
    }

    [Fact]
    public async Task Remove_ForwardsValidIdentifiersToRepository()
    {
        var repository = new FakeMembershipRepository();
        var halaqaId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var result = await new RemoveHalaqaMembershipUseCase(repository).ExecuteAsync(halaqaId, membershipId);

        Assert.True(result.IsSuccess);
        Assert.Equal(halaqaId, repository.Removal?.HalaqaId);
        Assert.Equal(membershipId, repository.Removal?.MembershipId);
    }

    private sealed class FakeMembershipRepository : IHalaqaMembershipRepository
    {
        public AssignStudentToHalaqaCommand? Assignment { get; private set; }
        public UpdateHalaqaMembershipCommand? Update { get; private set; }
        public (Guid HalaqaId, Guid MembershipId)? Removal { get; private set; }

        public Task<Result<MembershipPage>> ListAsync(Guid halaqaId, string? status = null, int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<MembershipPage>.Success(new MembershipPage(Array.Empty<HalaqaMembership>(), 1, 1, 20, 0)));

        public Task<Result<HalaqaMembership>> AssignAsync(AssignStudentToHalaqaCommand command, CancellationToken cancellationToken = default)
        {
            Assignment = command;
            return Task.FromResult(Result<HalaqaMembership>.Success(CreateMembership()));
        }

        public Task<Result<HalaqaMembership>> UpdateAsync(UpdateHalaqaMembershipCommand command, CancellationToken cancellationToken = default)
        {
            Update = command;
            return Task.FromResult(Result<HalaqaMembership>.Success(CreateMembership()));
        }

        public Task<Result> RemoveAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default)
        {
            Removal = (halaqaId, membershipId);
            return Task.FromResult(Result.Success());
        }

        private static HalaqaMembership CreateMembership() => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MembershipStudent(Guid.NewGuid(), "طالب", "student@example.test", null, "active", null, null),
            MembershipStatus.Active,
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
    }
}
