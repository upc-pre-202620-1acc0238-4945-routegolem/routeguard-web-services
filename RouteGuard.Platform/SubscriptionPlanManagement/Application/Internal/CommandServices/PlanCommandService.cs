using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.CommandServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.Internal.CommandServices;

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