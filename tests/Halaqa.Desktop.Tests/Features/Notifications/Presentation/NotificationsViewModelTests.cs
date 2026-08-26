using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Features.Notifications.Domain.Repositories;
using Halaqa.Desktop.Features.Notifications.Domain.UseCases;
using Halaqa.Desktop.Features.Notifications.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Notifications.Presentation;

public sealed class NotificationsViewModelTests
{
    [Fact]
    public async Task Load_PopulatesCurrentUserNotifications()
    {
        var repository = new FakeNotificationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize();

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Single(viewModel.Notifications);
        Assert.Equal(1, viewModel.Total);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task MarkSelectedRead_ReloadsOfficialListAfterSuccess()
    {
        var repository = new FakeNotificationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize();
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedNotification = Assert.Single(viewModel.Notifications);

        await viewModel.MarkSelectedReadCommand.ExecuteAsync(null);

        Assert.Equal(viewModel.SelectedNotification!.Id, repository.MarkedReadId);
        Assert.True(Assert.Single(viewModel.Notifications).IsRead);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task MarkAllRead_ReloadsListAfterSuccess()
    {
        var repository = new FakeNotificationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize();
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.MarkAllReadCommand.ExecuteAsync(null);

        Assert.True(repository.MarkAllReadCalled);
        Assert.True(Assert.Single(viewModel.Notifications).IsRead);
    }

    private static NotificationsViewModel CreateViewModel(INotificationRepository repository) => new(
        new ListNotificationsUseCase(repository),
        new MarkNotificationReadUseCase(repository),
        new MarkAllNotificationsReadUseCase(repository));

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public HalaqaNotification Notification { get; } = new(
            Guid.NewGuid(),
            NotificationType.Reminder,
            "تذكير",
            "رسالة اختبار",
            new NotificationPayload(null, NotificationEntityType.Halaqa, Guid.NewGuid(), null, null, NotificationAction.Open, null),
            null,
            DateTimeOffset.UtcNow);

        public Guid? MarkedReadId { get; private set; }
        public bool MarkAllReadCalled { get; private set; }

        public Task<Result<NotificationPage>> ListAsync(NotificationQuery query, CancellationToken cancellationToken = default)
        {
            var isRead = MarkedReadId is not null || MarkAllReadCalled;
            return Task.FromResult(Result<NotificationPage>.Success(new NotificationPage(
                new[] {Notification with { ReadAt = isRead ? DateTimeOffset.UtcNow : null }},
                1, 1, 20, 1)));
        }

        public Task<Result> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
        {
            MarkedReadId = notificationId;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default)
        {
            MarkAllReadCalled = true;
            return Task.FromResult(Result.Success());
        }
    }
}
