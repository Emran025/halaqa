using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Features.Notifications.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Domain.UseCases;

public sealed class ListNotificationsUseCase(INotificationRepository repository)
{
    public Task<Result<NotificationPage>> ExecuteAsync(NotificationQuery query, CancellationToken cancellationToken = default) =>
        repository.ListAsync(query, cancellationToken);
}

public sealed class MarkNotificationReadUseCase(INotificationRepository repository)
{
    public Task<Result> ExecuteAsync(Guid notificationId, CancellationToken cancellationToken = default) =>
        repository.MarkReadAsync(notificationId, cancellationToken);
}

public sealed class MarkAllNotificationsReadUseCase(INotificationRepository repository)
{
    public Task<Result> ExecuteAsync(CancellationToken cancellationToken = default) =>
        repository.MarkAllReadAsync(cancellationToken);
}
