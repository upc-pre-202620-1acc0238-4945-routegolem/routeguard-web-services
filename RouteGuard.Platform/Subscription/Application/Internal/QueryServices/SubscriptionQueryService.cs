using RouteGuard.Platform.Subscription.Application.QueryServices;
using RouteGuard.Platform.Subscription.Domain.Model.Queries;
using RouteGuard.Platform.Subscription.Domain.Model.ValueObjects;
using RouteGuard.Platform.Subscription.Domain.Repositories;
using OrganizationId = RouteGuard.Platform.Subscription.Domain.Model.ValueObjects.OrganizationId;
using SubscriptionAggregate = RouteGuard.Platform.Subscription.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.Subscription.Application.Internal.QueryServices;

public class SubscriptionQueryService(ISubscriptionRepository subscriptionRepository)
    : ISubscriptionQueryService
{
    public Task<SubscriptionAggregate?> Handle(GetSubscriptionByIdQuery query, CancellationToken cancellationToken) =>
        subscriptionRepository.FindBySubscriptionIdAsync(new SubscriptionId(query.SubscriptionId), cancellationToken);

    public async Task<IEnumerable<SubscriptionAggregate>> Handle(GetAllSubscriptionsQuery query,
        CancellationToken cancellationToken) =>
        await subscriptionRepository.ListAsync(cancellationToken);

    public Task<IEnumerable<SubscriptionAggregate>> Handle(GetSubscriptionsByOrganizationIdQuery query,
        CancellationToken cancellationToken) =>
        subscriptionRepository.FindByOrganizationIdAsync(new OrganizationId(query.OrganizationId), cancellationToken);
}
