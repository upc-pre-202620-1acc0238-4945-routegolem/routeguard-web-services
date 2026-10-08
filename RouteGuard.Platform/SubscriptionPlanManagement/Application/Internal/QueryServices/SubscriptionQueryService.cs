using RouteGuard.Platform.SubscriptionPlanManagement.Application.QueryServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Queries;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;
using OrganizationId = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects.OrganizationId;
using SubscriptionAggregate = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.Internal.QueryServices;

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
