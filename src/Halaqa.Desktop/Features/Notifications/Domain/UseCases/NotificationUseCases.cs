using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Features.Notifications.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Domain.UseCases;

public sealed class ListNotificationsUseCase
{

    private readonly INotificationRepository repository;


    public ListNotificationsUseCase(

        INotificationRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<NotificationPage>> ExecuteAsync(NotificationQuery query, CancellationToken cancellationToken = default) =>
        repository.ListAsync(query, cancellationToken);
}

public sealed class MarkNotificationReadUseCase
{

    private readonly INotificationRepository repository;


    public MarkNotificationReadUseCase(

        INotificationRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(Guid notificationId, CancellationToken cancellationToken = default) =>
        repository.MarkReadAsync(notificationId, cancellationToken);
}

public sealed class MarkAllNotificationsReadUseCase
{

    private readonly INotificationRepository repository;


    public MarkAllNotificationsReadUseCase(

        INotificationRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(CancellationToken cancellationToken = default) =>
        repository.MarkAllReadAsync(cancellationToken);
}
