using Halaqa.Desktop.Features.Notifications.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Notifications.Data.Mappers;
using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Features.Notifications.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Data.Repositories;

internal sealed class NotificationRepository : INotificationRepository
{

    private readonly INotificationRemoteDataSource remoteDataSource;


    public NotificationRepository(

        INotificationRemoteDataSource remoteDataSource

    )

    {

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<NotificationPage>> ListAsync(NotificationQuery query, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.ListAsync(query, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? NotificationMapper.ToDomain(response.Value)
            : Result<NotificationPage>.Failure(response.Error!);
    }

    public Task<Result> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default) =>
        remoteDataSource.MarkReadAsync(notificationId, cancellationToken);

    public Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default) =>
        remoteDataSource.MarkAllReadAsync(cancellationToken);
}
