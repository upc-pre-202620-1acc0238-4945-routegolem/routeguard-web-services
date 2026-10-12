using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.CommandServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;
using Stripe;
using SubscriptionAggregate = RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates.Subscription;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Application.Internal.CommandServices;

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

    public async Task<Result<PaymentIntentResource>> Handle(CreatePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await planRepository.FindByPlanIdAsync(new PlanId(command.PlanId), cancellationToken);
            if (plan is null)
                return Result<PaymentIntentResource>.Failure(SubscriptionError.PlanNotFound, "Plan was not found.");

            // Convert price to cents (minimum 50 cents for Stripe)
            var amountInCents = Math.Max(50L, (long)Math.Round(plan.Price * 100m));
            const string currency = "usd";

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Description = $"RouteGuard Subscription - Plan {plan.GetPlanName()}",
                Metadata = new Dictionary<string, string>
                {
                    ["organizationId"] = command.OrganizationId.ToString(),
                    ["planId"] = command.PlanId.ToString(),
                    ["planTier"] = plan.GetPlanName()
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return Result<PaymentIntentResource>.Success(
                new PaymentIntentResource(intent.ClientSecret, intent.Id, intent.Amount, intent.Currency));
        }
        catch (StripeException ex)
        {
            return Result<PaymentIntentResource>.Failure(SubscriptionError.InternalServerError, $"Stripe error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<PaymentIntentResource>.Failure(SubscriptionError.InternalServerError, ex.Message);
        }
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