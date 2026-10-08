using RouteGuard.Platform.SubscriptionPlanManagement.Application.QueryServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Queries;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.Internal.QueryServices;

public class PlanQueryService(IPlanRepository planRepository)
    : IPlanQueryService
{
    public Task<Plan?> Handle(GetPlanByIdQuery query, CancellationToken cancellationToken) =>
        planRepository.FindByPlanIdAsync(new PlanId(query.PlanId), cancellationToken);

    public async Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken) =>
        await planRepository.ListAsync(cancellationToken);
}
