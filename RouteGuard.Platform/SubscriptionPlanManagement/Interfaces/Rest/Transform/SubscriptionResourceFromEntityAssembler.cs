using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;
using SubscriptionAggregate = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static PlanResource ToResourceFromEntity(Plan plan) =>
        new(plan.Id.Identifier, plan.Tier.Value, plan.RouteQuota.MaxRoutes, plan.DriverQuota.MaxDrivers, plan.Price);

    public static SubscriptionResource ToResourceFromEntity(SubscriptionAggregate subscription) =>
        new(subscription.Id.Identifier, subscription.OrganizationId.Identifier, subscription.PlanId.Identifier,
            subscription.State.Value, subscription.StartDate, subscription.EndDate, subscription.GetRemainingDays());
}
