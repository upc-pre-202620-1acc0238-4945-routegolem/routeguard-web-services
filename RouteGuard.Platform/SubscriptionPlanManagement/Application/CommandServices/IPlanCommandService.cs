using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.CommandServices;

public interface IPlanCommandService
{
    Task<Result<Plan>> Handle(CreatePlanCommand command, CancellationToken cancellationToken);
}