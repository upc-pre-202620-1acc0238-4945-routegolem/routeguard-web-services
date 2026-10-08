using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;
using SubscriptionAggregate = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Infraestructure.Persistence.EntityFrameworkCore.Repositories;

public class SubscriptionRepository(AppDbContext context)
    : BaseRepository<SubscriptionAggregate>(context), ISubscriptionRepository
{
    public Task<SubscriptionAggregate?> FindBySubscriptionIdAsync(SubscriptionId subscriptionId,
        CancellationToken cancellationToken) =>
        Context.Set<SubscriptionAggregate>()
            .FirstOrDefaultAsync(subscription => subscription.Id == subscriptionId, cancellationToken);

    public async Task<IEnumerable<SubscriptionAggregate>> FindByOrganizationIdAsync(OrganizationId organizationId,
        CancellationToken cancellationToken) =>
        await Context.Set<SubscriptionAggregate>()
            .Where(subscription => subscription.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
}
