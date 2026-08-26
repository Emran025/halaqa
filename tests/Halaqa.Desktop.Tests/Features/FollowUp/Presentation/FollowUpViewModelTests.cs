using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.Repositories;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.FollowUp.Presentation;

public sealed class FollowUpViewModelTests
{
    [Fact]
    public async Task Load_UsesInitializedStudentAndPopulatesOfficialFollowUpData()
    {
        var repository = new FakeFollowUpRepository();
        var viewModel = CreateViewModel(repository);
        var studentId = Guid.NewGuid();
        viewModel.Initialize(studentId);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(studentId, repository.LastPlanStudentId);
        Assert.Equal(studentId, repository.LastAvailabilityStudentId);
        Assert.Equal(studentId, repository.LastTrackingsStudentId);
        Assert.NotNull(viewModel.Plan);
        Assert.NotNull(viewModel.Availability);
        Assert.Single(viewModel.Items);
        Assert.Single(viewModel.Trackings);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task CompleteSelectedItem_UsesFreshClientOperationIdAndUpdatesItem()
    {
        var repository = new FakeFollowUpRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid());
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedItem = Assert.Single(viewModel.Items);

        await viewModel.CompleteSelectedItemCommand.ExecuteAsync(null);

        Assert.Equal(viewModel.SelectedItem.Id, repository.CompletedItemId);
        Assert.NotEqual(Guid.Empty, repository.CompleteOperationId);
        Assert.Equal(FollowUpItemState.Completed, viewModel.SelectedItem.State);
        Assert.False(viewModel.IsError);
    }

    private static FollowUpViewModel CreateViewModel(IFollowUpRepository repository) => new(
        new GetFollowUpPlanUseCase(repository),
        new UpdateFollowUpPlanUseCase(repository),
        new GetAvailabilityUseCase(repository),
        new UpdateAvailabilityUseCase(repository),
        new ListFollowUpItemsUseCase(repository),
        new CompleteFollowUpItemUseCase(repository),
        new SkipFollowUpItemUseCase(repository),
        new RescheduleFollowUpItemUseCase(repository),
        new ListStudentTrackingsUseCase(repository));

    private sealed class FakeFollowUpRepository : IFollowUpRepository
    {
        private readonly Guid _planId = Guid.NewGuid();
        private readonly Guid _detailId = Guid.NewGuid();
        private readonly Guid _itemId = Guid.NewGuid();

        public Guid LastPlanStudentId { get; private set; }
        public Guid LastAvailabilityStudentId { get; private set; }
        public Guid LastTrackingsStudentId { get; private set; }
        public Guid? CompletedItemId { get; private set; }
        public Guid CompleteOperationId { get; private set; }

        public Task<Result<FollowUpPlan>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            LastPlanStudentId = studentId;
            return Task.FromResult(Result<FollowUpPlan>.Success(CreatePlan(studentId)));
        }

        public Task<Result<FollowUpPlan>> UpdatePlanAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<FollowUpPlan>.Success(CreatePlan(command.StudentId)));

        public Task<Result<AttendancePreferences>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            LastAvailabilityStudentId = studentId;
            return Task.FromResult(Result<AttendancePreferences>.Success(CreateAvailability()));
        }

        public Task<Result<AttendancePreferences>> UpdateAvailabilityAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<AttendancePreferences>.Success(command.Preferences));

        public Task<Result<FollowUpItemPage>> ListItemsAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<FollowUpItemPage>.Success(new FollowUpItemPage(new[] { CreateItem(FollowUpItemState.Due) }, 1, 1, 20, 1)));

        public Task<Result<FollowUpItem>> CompleteItemAsync(Guid itemId, Guid clientOperationId, CancellationToken cancellationToken = default)
        {
            CompletedItemId = itemId;
            CompleteOperationId = clientOperationId;
            return Task.FromResult(Result<FollowUpItem>.Success(CreateItem(FollowUpItemState.Completed)));
        }

        public Task<Result<FollowUpItem>> SkipItemAsync(Guid itemId, string reason, Guid clientOperationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<FollowUpItem>.Success(CreateItem(FollowUpItemState.Skipped) with { SkipReason = reason }));

        public Task<Result<FollowUpItem>> RescheduleItemAsync(RescheduleFollowUpItemCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<FollowUpItem>.Success(CreateItem(FollowUpItemState.Upcoming) with { ScheduledFor = command.ScheduledAt }));

        public Task<Result<TrackingPage>> ListTrackingsAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default)
        {
            LastTrackingsStudentId = studentId;
            return Task.FromResult(Result<TrackingPage>.Success(new TrackingPage(
                [new TrackingItem(Guid.NewGuid(), studentId, null, new DateOnly(2026, 8, 25), AttendanceType.Present, "مراجعة", 90, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)],
                1, 1, 20, 1)));
        }

        private FollowUpPlan CreatePlan(Guid studentId) => new(
            _planId,
            studentId,
            Guid.NewGuid(),
            null,
            FollowUpFrequency.Daily,
            "active",
            "Asia/Riyadh",
            [CreateDetail()],
            CreateAvailability(),
            new DateOnly(2026, 8, 1),
            null,
            1,
            null,
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        private AttendancePreferences CreateAvailability() => new(
            "Asia/Riyadh",
            [new WeeklyAvailabilitySlot(0, new TimeOnly(18, 0), new TimeOnly(18, 30), true)],
            30);

        private FollowUpPlanDetail CreateDetail() => new(
            _detailId,
            FollowUpTaskType.Memorization,
            FollowUpUnit.Page,
            2,
            "حفظ",
            1,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        private FollowUpItem CreateItem(FollowUpItemState state) => new(
            _itemId,
            _planId,
            _detailId,
            Guid.NewGuid(),
            null,
            FollowUpTaskType.Memorization,
            CreateDetail(),
            DateTimeOffset.UtcNow,
            "Asia/Riyadh",
            state,
            state == FollowUpItemState.Completed ? DateTimeOffset.UtcNow : null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
    }
}
