namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;

public record PlanResource(Guid Id, string PlanTier, int MaxRoutes, int MaxDrivers, decimal Price);
