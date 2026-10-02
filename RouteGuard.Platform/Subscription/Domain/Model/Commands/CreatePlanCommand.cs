namespace RouteGuard.Platform.Subscription.Domain.Model.Commands;

public record CreatePlanCommand(string PlanTier, int MaxRoutes, int MaxDrivers, decimal Price);
