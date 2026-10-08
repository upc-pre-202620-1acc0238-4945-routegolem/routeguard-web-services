using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Queries;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.QueryServices;

public interface IPlanQueryService
{
    Task<Plan?> Handle(GetPlanByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken);
}
