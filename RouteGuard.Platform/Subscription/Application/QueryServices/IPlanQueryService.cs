using RouteGuard.Platform.Subscription.Domain.Model.Aggregates;
using RouteGuard.Platform.Subscription.Domain.Model.Queries;

namespace RouteGuard.Platform.Subscription.Application.QueryServices;

public interface IPlanQueryService
{
    Task<Plan?> Handle(GetPlanByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken);
}
