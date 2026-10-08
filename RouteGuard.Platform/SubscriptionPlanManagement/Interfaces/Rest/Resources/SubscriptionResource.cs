namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;

public record SubscriptionResource(
    Guid Id,
    Guid OrganizationId,
    Guid PlanId,
    string State,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int RemainingDays);
