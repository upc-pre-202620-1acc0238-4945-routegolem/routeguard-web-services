namespace RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;

public record UpgradeSubscriptionCommand(Guid SubscriptionId, Guid PlanId);
