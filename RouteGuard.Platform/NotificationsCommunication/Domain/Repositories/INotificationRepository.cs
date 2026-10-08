using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.Shared.Domain.Repositories;

namespace RouteGuard.Platform.NotificationsCommunication.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<Notification?> FindByIdAsync(Guid id);
    Task<IEnumerable<Notification>> FindByParentIdAsync(Guid parentId);
}
