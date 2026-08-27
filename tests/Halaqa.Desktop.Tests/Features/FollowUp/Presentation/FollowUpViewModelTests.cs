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
    public async Task SavePlan_SendsEveryConfiguredPlanDetail()
    {
        var repository = new FakeFollowUpRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid());
        viewModel.Frequency = FollowUpFrequency.TwiceAWeek;
        viewModel.PlanDetails[0].TaskType = FollowUpTaskType.Memorization;
        viewModel.PlanDetails[0].Unit = FollowUpUnit.Page;
        viewModel.PlanDetails[0].Amount = "2";
        viewModel.PlanDetails[0].Notes = "حفظ";
        viewModel.AddPlanDetailCommand.Execute(null);
        viewModel.PlanDetails[1].TaskType = FollowUpTaskType.Review;
        viewModel.PlanDetails[1].Unit = FollowUpUnit.Hizb;
        viewModel.PlanDetails[1].Amount = "0.5";
        viewModel.PlanDetails[1].Notes = "مراجعة";

        await viewModel.SavePlanCommand.ExecuteAsync(null);

        var command = Assert.IsType<UpdateFollowUpPlanCommand>(repository.LastUpdatePlanCommand);
        Assert.Equal(FollowUpFrequency.TwiceAWeek, command.Frequency);
        Assert.Equal(2, command.Details.Count);
        Assert.Collection(command.Details,
            detail => Assert.Equal((FollowUpTaskType.Memorization, FollowUpUnit.Page, 2m, "حفظ"), (detail.TaskType, detail.Unit, detail.Amount, detail.Notes)),
            detail => Assert.Equal((FollowUpTaskType.Review, FollowUpUnit.Hizb, 0.5m, "مراجعة"), (detail.TaskType, detail.Unit, detail.Amount, detail.Notes)));
    }

    [Fact]
    public async Task SaveAvailability_SendsEveryConfiguredWeeklySlot()
    {
        var repository = new FakeFollowUpRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid());
        viewModel.Timezone = "Asia/Riyadh";
        viewModel.PreferredSessionDurationMinutes = "45";
        viewModel.WeeklySlots[0].DayOfWeek = 0;
        viewModel.WeeklySlots[0].From = "18:00";
        viewModel.WeeklySlots[0].To = "18:30";
        viewModel.AddAvailabilitySlotCommand.Execute(null);
        viewModel.WeeklySlots[1].DayOfWeek = 4;
        viewModel.WeeklySlots[1].From = "20:00";
        viewModel.WeeklySlots[1].To = "20:45";
        viewModel.WeeklySlots[1].Preferred = false;

        await viewModel.SaveAvailabilityCommand.ExecuteAsync(null);

        var command = Assert.IsType<UpdateAvailabilityCommand>(repository.LastUpdateAvailabilityCommand);
        Assert.Equal("Asia/Riyadh", command.Preferences.Timezone);
        Assert.Equal(45, command.Preferences.PreferredSessionDurationMinutes);
        Assert.Collection(command.Preferences.WeeklySlots,
            slot => Assert.Equal((0, new TimeOnly(18, 0), new TimeOnly(18, 30), true), (slot.DayOfWeek, slot.From, slot.To, slot.Preferred)),
            slot => Assert.Equal((4, new TimeOnly(20, 0), new TimeOnly(20, 45), false), (slot.DayOfWeek, slot.From, slot.To, slot.Preferred)));
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
        public UpdateFollowUpPlanCommand? LastUpdatePlanCommand { get; private set; }
        public UpdateAvailabilityCommand? LastUpdateAvailabilityCommand { get; private set; }

        public Task<Result<FollowUpPlan>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            LastPlanStudentId = studentId;
            return Task.FromResult(Result<FollowUpPlan>.Success(CreatePlan(studentId)));
        }

        public Task<Result<FollowUpPlan>> UpdatePlanAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdatePlanCommand = command;
            return Task.FromResult(Result<FollowUpPlan>.Success(CreatePlan(command.StudentId)));
        }

        public Task<Result<AttendancePreferences>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            LastAvailabilityStudentId = studentId;
            return Task.FromResult(Result<AttendancePreferences>.Success(CreateAvailability()));
        }

        public Task<Result<AttendancePreferences>> UpdateAvailabilityAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateAvailabilityCommand = command;
            return Task.FromResult(Result<AttendancePreferences>.Success(command.Preferences));
        }

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
                new[] {new TrackingItem(Guid.NewGuid(), studentId, null, new DateOnly(2026, 8, 25), AttendanceType.Present, "مراجعة", 90, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)},
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
            new[] {CreateDetail()},
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
            new[] {new WeeklyAvailabilitySlot(0, new TimeOnly(18, 0), new TimeOnly(18, 30), true)},
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
