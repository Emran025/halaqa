using Halaqa.Desktop.Features.Notifications.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Notifications.Data.Repositories;
using Halaqa.Desktop.Features.Notifications.Domain.Repositories;
using Halaqa.Desktop.Features.Notifications.Domain.UseCases;
using Halaqa.Desktop.Features.Notifications.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Notifications;

public static class NotificationsFeatureModule
{
    public static IServiceCollection AddNotificationsFeature(this IServiceCollection services)
    {
        services.AddSingleton<INotificationRemoteDataSource, NotificationRemoteDataSource>();
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<ListNotificationsUseCase>();
        services.AddSingleton<MarkNotificationReadUseCase>();
        services.AddSingleton<MarkAllNotificationsReadUseCase>();
        services.AddSingleton<NotificationsViewModel>();
        return services;
    }
}
