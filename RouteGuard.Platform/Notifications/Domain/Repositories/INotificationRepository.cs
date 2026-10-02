using RouteGuard.Platform.Notifications.Domain.Model.Aggregates;
using RouteGuard.Platform.Shared.Domain.Repositories;

namespace RouteGuard.Platform.Notifications.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<Notification?> FindByIdAsync(Guid id);
    Task<IEnumerable<Notification>> FindByParentIdAsync(Guid parentId);
}
