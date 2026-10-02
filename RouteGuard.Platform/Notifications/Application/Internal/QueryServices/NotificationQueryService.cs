using RouteGuard.Platform.Notifications.Application.QueryServices;
using RouteGuard.Platform.Notifications.Domain.Model.Aggregates;
using RouteGuard.Platform.Notifications.Domain.Model.Queries;
using RouteGuard.Platform.Notifications.Domain.Repositories;

namespace RouteGuard.Platform.Notifications.Application.Internal.QueryServices;

public class NotificationQueryService(INotificationRepository notificationRepository) : INotificationQueryService
{
    public async Task<Notification?> Handle(GetNotificationByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await notificationRepository.FindByIdAsync(query.NotificationId);
    }

    public async Task<IEnumerable<Notification>> Handle(GetAllNotificationsQuery query, CancellationToken cancellationToken = default)
    {
        return await notificationRepository.ListAsync();
    }

    public async Task<IEnumerable<Notification>> Handle(GetNotificationsByParentIdQuery query, CancellationToken cancellationToken = default)
    {
        return await notificationRepository.FindByParentIdAsync(query.ParentId);
    }
}
