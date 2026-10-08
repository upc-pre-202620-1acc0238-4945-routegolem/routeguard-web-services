using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;
using SubscriptionAggregate = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.CommandServices;

public interface ISubscriptionCommandService
{
    Task<Result<SubscriptionAggregate>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken);
    Task<Result<SubscriptionAggregate>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken);
    Task<Result<SubscriptionAggregate>> Handle(UpgradeSubscriptionCommand command, CancellationToken cancellationToken);
    Task<Result<SubscriptionAggregate>> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken);
}