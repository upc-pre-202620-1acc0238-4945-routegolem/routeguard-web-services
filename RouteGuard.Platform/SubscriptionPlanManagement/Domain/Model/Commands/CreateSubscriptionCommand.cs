namespace RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;

public record CreateSubscriptionCommand(Guid OrganizationId, Guid PlanId, DateTimeOffset StartDate, DateTimeOffset EndDate);
