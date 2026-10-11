using Microsoft.AspNetCore.Authorization;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.CommandServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.QueryServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Commands;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Queries;
using RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;
using RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Subscription management endpoints.")]
public class SubscriptionsController(
    ISubscriptionCommandService commandService,
    ISubscriptionQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateSubscription(CreateSubscriptionResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new CreateSubscriptionCommand(resource.OrganizationId,
            resource.PlanId, resource.StartDate, resource.EndDate), cancellationToken);
        return SubscriptionActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            subscription => CreatedAtAction(nameof(GetSubscriptionById),
                new { subscriptionId = subscription.Id.Identifier },
                SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("{subscriptionId:guid}")]
    public async Task<IActionResult> GetSubscriptionById(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await queryService.Handle(new GetSubscriptionByIdQuery(subscriptionId), cancellationToken);
        if (subscription is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                SubscriptionError.SubscriptionNotFound, "Subscription was not found.");
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetSubscriptions([FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var subscriptions = organizationId is null
            ? await queryService.Handle(new GetAllSubscriptionsQuery(), cancellationToken)
            : await queryService.Handle(new GetSubscriptionsByOrganizationIdQuery(organizationId.Value),
                cancellationToken);
        return Ok(subscriptions.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{subscriptionId:guid}/activate")]
    public async Task<IActionResult> ActivateSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new ActivateSubscriptionCommand(subscriptionId), cancellationToken);
        return SubscriptionActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            subscription => Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{subscriptionId:guid}/plan")]
    public async Task<IActionResult> UpgradeSubscription(Guid subscriptionId, UpgradeSubscriptionResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new UpgradeSubscriptionCommand(subscriptionId, resource.PlanId),
            cancellationToken);
        return SubscriptionActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            subscription => Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{subscriptionId:guid}/cancel")]
    public async Task<IActionResult> CancelSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new CancelSubscriptionCommand(subscriptionId), cancellationToken);
        return SubscriptionActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            subscription => Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription)));
    }
}
