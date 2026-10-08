

using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;

public interface IPlanRepository : IBaseRepository<Plan>
{
    Task<Plan?> FindByPlanIdAsync(PlanId planId, CancellationToken cancellationToken);
}
