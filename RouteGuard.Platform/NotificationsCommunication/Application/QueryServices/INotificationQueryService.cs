using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Queries;

namespace RouteGuard.Platform.NotificationsCommunication.Application.QueryServices;

public interface INotificationQueryService
{
    Task<Notification?> Handle(GetNotificationByIdQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> Handle(GetAllNotificationsQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> Handle(GetNotificationsByParentIdQuery query, CancellationToken cancellationToken = default);
}
