using RouteGuard.Platform.Notifications.Domain.Model.Aggregates;
using RouteGuard.Platform.Notifications.Domain.Model.Queries;

namespace RouteGuard.Platform.Notifications.Application.QueryServices;

public interface INotificationQueryService
{
    Task<Notification?> Handle(GetNotificationByIdQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> Handle(GetAllNotificationsQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> Handle(GetNotificationsByParentIdQuery query, CancellationToken cancellationToken = default);
}
