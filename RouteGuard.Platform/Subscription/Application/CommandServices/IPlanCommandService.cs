using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Subscription.Domain.Model.Aggregates;
using RouteGuard.Platform.Subscription.Domain.Model.Commands;

namespace RouteGuard.Platform.Subscription.Application.CommandServices;

public interface IPlanCommandService
{
    Task<Result<Plan>> Handle(CreatePlanCommand command, CancellationToken cancellationToken);
}