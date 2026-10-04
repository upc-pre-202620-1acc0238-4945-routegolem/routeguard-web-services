using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Subscription.Application.CommandServices;
using RouteGuard.Platform.Subscription.Domain.Model;
using RouteGuard.Platform.Subscription.Domain.Model.Aggregates;
using RouteGuard.Platform.Subscription.Domain.Model.Commands;
using RouteGuard.Platform.Subscription.Domain.Repositories;

namespace RouteGuard.Platform.Subscription.Application.Internal.CommandServices;

public class PlanCommandService(
    IPlanRepository planRepository,
    IUnitOfWork unitOfWork) : IPlanCommandService
{
    public async Task<Result<Plan>> Handle(CreatePlanCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plan = new Plan(command);
            await planRepository.AddAsync(plan, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Plan>.Success(plan);
        }
        catch (ArgumentException ex) { return Result<Plan>.Failure(SubscriptionError.InvalidSubscriptionData, ex.Message); }
    }
}