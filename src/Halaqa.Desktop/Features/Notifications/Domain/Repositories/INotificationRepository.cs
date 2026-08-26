using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Domain.Repositories;

public interface INotificationRepository
{
    Task<Result<NotificationPage>> ListAsync(NotificationQuery query, CancellationToken cancellationToken = default);
    Task<Result> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default);
}
