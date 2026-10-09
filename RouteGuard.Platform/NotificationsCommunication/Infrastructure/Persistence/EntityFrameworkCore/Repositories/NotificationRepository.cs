using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects;
using RouteGuard.Platform.NotificationsCommunication.Domain.Repositories;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RouteGuard.Platform.NotificationsCommunication.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class NotificationRepository(AppDbContext context)
    : BaseRepository<Notification>(context), INotificationRepository
{
    public override async Task<IEnumerable<Notification>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Notification>()
            .Include(n => n.Alerts)
            .Include(n => n.Announcements)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> FindByIdAsync(Guid id)
    {
        var notificationId = new NotificationId(id);
        return await Context.Set<Notification>()
            .Include(n => n.Alerts)
            .Include(n => n.Announcements)
            .AsSplitQuery()
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task<IEnumerable<Notification>> FindByParentIdAsync(Guid parentId)
    {
        var parentIdVo = new ParentId(parentId);
        return await Context.Set<Notification>()
            .Include(n => n.Alerts)
            .Include(n => n.Announcements)
            .AsSplitQuery()
            .Where(n => n.ParentId == parentIdVo)
            .ToListAsync();
    }
}
