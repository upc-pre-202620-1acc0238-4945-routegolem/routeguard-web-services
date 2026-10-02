using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Subscription.Domain.Model.Aggregates;
using RouteGuard.Platform.Subscription.Domain.Model.Commands;
using SubscriptionAggregate = RouteGuard.Platform.Subscription.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.Subscription.Application.CommandServices;

public interface ISubscriptionCommandService
{
    Task<Result<Plan>> Handle(CreatePlanCommand command, CancellationToken cancellationToken);

    Task<Result<SubscriptionAggregate>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken);

    Task<Result<SubscriptionAggregate>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken);

    Task<Result<SubscriptionAggregate>> Handle(UpgradeSubscriptionCommand command, CancellationToken cancellationToken);

    Task<Result<SubscriptionAggregate>> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken);
}
