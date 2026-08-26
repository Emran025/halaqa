using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Notifications.Data.Models;
using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Data.DataSources.Remote;

internal interface INotificationRemoteDataSource
{
    Task<Result<NotificationCollectionResponseDto>> ListAsync(NotificationQuery query, CancellationToken cancellationToken = default);
    Task<Result> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default);
}

internal sealed class NotificationRemoteDataSource : INotificationRemoteDataSource
{

    private readonly IApiClient apiClient;


    public NotificationRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<NotificationCollectionResponseDto>> ListAsync(NotificationQuery query, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<NotificationCollectionResponseDto>(
            $"notifications?unread_only={query.UnreadOnly.ToString().ToLowerInvariant()}&page={query.Page}&per_page={query.PerPage}",
            cancellationToken);

    public Task<Result> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync($"notifications/{notificationId}/read", new { }, cancellationToken);

    public Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default) =>
        apiClient.PostAsync("notifications/read-all", new { }, cancellationToken);
}
