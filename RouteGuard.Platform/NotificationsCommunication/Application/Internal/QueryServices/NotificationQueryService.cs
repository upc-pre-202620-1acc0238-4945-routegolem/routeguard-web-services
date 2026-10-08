using RouteGuard.Platform.NotificationsCommunication.Application.QueryServices;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Queries;
using RouteGuard.Platform.NotificationsCommunication.Domain.Repositories;

namespace RouteGuard.Platform.NotificationsCommunication.Application.Internal.QueryServices;

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
