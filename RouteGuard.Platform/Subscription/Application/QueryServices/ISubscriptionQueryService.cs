using RouteGuard.Platform.Subscription.Domain.Model.Queries;
using SubscriptionAggregate = RouteGuard.Platform.Subscription.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.Subscription.Application.QueryServices;

public interface ISubscriptionQueryService
{

    Task<SubscriptionAggregate?> Handle(GetSubscriptionByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<SubscriptionAggregate>> Handle(GetAllSubscriptionsQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<SubscriptionAggregate>> Handle(GetSubscriptionsByOrganizationIdQuery query,
        CancellationToken cancellationToken);
}
