using RouteGuard.Platform.Subscription.Application.QueryServices;
using RouteGuard.Platform.Subscription.Domain.Model.Aggregates;
using RouteGuard.Platform.Subscription.Domain.Model.Queries;
using RouteGuard.Platform.Subscription.Domain.Model.ValueObjects;
using RouteGuard.Platform.Subscription.Domain.Repositories;

namespace RouteGuard.Platform.Subscription.Application.Internal.QueryServices;

public class PlanQueryService(IPlanRepository planRepository)
    : IPlanQueryService
{
    public Task<Plan?> Handle(GetPlanByIdQuery query, CancellationToken cancellationToken) =>
        planRepository.FindByPlanIdAsync(new PlanId(query.PlanId), cancellationToken);

    public async Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken) =>
        await planRepository.ListAsync(cancellationToken);
}
