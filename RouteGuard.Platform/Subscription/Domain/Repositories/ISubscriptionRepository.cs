using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Subscription.Domain.Model.ValueObjects;
using SubscriptionAggregate = RouteGuard.Platform.Subscription.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.Subscription.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<SubscriptionAggregate>
{
    Task<SubscriptionAggregate?> FindBySubscriptionIdAsync(SubscriptionId subscriptionId,
        CancellationToken cancellationToken);

    Task<IEnumerable<SubscriptionAggregate>> FindByOrganizationIdAsync(OrganizationId organizationId,
        CancellationToken cancellationToken);
}
