using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Subscription.Application.CommandServices;
using RouteGuard.Platform.Subscription.Domain.Model;
using RouteGuard.Platform.Subscription.Domain.Model.Commands;
using RouteGuard.Platform.Subscription.Domain.Model.ValueObjects;
using RouteGuard.Platform.Subscription.Domain.Repositories;
using SubscriptionAggregate = RouteGuard.Platform.Subscription.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.Subscription.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : ISubscriptionCommandService
{
    public async Task<Result<SubscriptionAggregate>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await planRepository.FindByPlanIdAsync(new PlanId(command.PlanId), cancellationToken);
            if (plan is null) return Result<SubscriptionAggregate>.Failure(SubscriptionError.PlanNotFound, "Plan was not found.");
            var subscription = new SubscriptionAggregate(command);
            await subscriptionRepository.AddAsync(subscription, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<SubscriptionAggregate>.Success(subscription);
        }
        catch (ArgumentException ex) { return Result<SubscriptionAggregate>.Failure(SubscriptionError.InvalidSubscriptionData, ex.Message); }
        catch (DbUpdateException) { return Result<SubscriptionAggregate>.Failure(SubscriptionError.DatabaseError, "Database error."); }
    }

    public Task<Result<SubscriptionAggregate>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(command.SubscriptionId, subscription => subscription.Activate(), cancellationToken);

    public Task<Result<SubscriptionAggregate>> Handle(UpgradeSubscriptionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(command.SubscriptionId, subscription => subscription.Upgrade(new PlanId(command.PlanId)), cancellationToken);

    public Task<Result<SubscriptionAggregate>> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(command.SubscriptionId, subscription => subscription.Cancel(), cancellationToken);

    private async Task<Result<SubscriptionAggregate>> MutateAsync(Guid subscriptionId, Action<SubscriptionAggregate> mutation, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.FindBySubscriptionIdAsync(new SubscriptionId(subscriptionId), cancellationToken);
        if (subscription is null) return Result<SubscriptionAggregate>.Failure(SubscriptionError.SubscriptionNotFound, "Subscription was not found.");
        mutation(subscription);
        subscriptionRepository.Update(subscription);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<SubscriptionAggregate>.Success(subscription);
    }
}