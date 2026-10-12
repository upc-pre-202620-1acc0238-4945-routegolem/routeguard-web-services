namespace RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;

public record CreatePaymentIntentCommand(Guid OrganizationId, Guid PlanId);
