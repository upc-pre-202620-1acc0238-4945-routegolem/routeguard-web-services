namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;

public record CreateSubscriptionResource(Guid OrganizationId, Guid PlanId, DateTimeOffset StartDate, DateTimeOffset EndDate);
