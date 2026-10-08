using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using SubscriptionAggregate = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<SubscriptionAggregate>
{
    Task<SubscriptionAggregate?> FindBySubscriptionIdAsync(SubscriptionId subscriptionId,
        CancellationToken cancellationToken);

    Task<IEnumerable<SubscriptionAggregate>> FindByOrganizationIdAsync(OrganizationId organizationId,
        CancellationToken cancellationToken);
}
