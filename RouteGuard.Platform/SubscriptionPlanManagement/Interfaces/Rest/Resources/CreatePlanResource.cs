namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;

public record CreatePlanResource(string PlanTier, int MaxRoutes, int MaxDrivers, decimal Price);
