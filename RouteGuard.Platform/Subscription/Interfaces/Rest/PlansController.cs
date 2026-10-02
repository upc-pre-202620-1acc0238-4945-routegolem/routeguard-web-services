using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.Subscription.Application.CommandServices;
using RouteGuard.Platform.Subscription.Application.QueryServices;
using RouteGuard.Platform.Subscription.Domain.Model;
using RouteGuard.Platform.Subscription.Domain.Model.Commands;
using RouteGuard.Platform.Subscription.Domain.Model.Queries;
using RouteGuard.Platform.Subscription.Interfaces.Rest.Resources;
using RouteGuard.Platform.Subscription.Interfaces.Rest.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.Subscription.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Subscription plan endpoints.")]
public class PlansController(
    ISubscriptionCommandService commandService,
    ISubscriptionQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePlan(CreatePlanResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new CreatePlanCommand(resource.PlanTier, resource.MaxRoutes,
            resource.MaxDrivers, resource.Price), cancellationToken);
        return SubscriptionActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            plan => CreatedAtAction(nameof(GetPlanById), new { planId = plan.Id.Identifier },
                SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(plan)));
    }

    [HttpGet("{planId:guid}")]
    public async Task<IActionResult> GetPlanById(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await queryService.Handle(new GetPlanByIdQuery(planId), cancellationToken);
        if (plan is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                SubscriptionError.PlanNotFound, "Plan was not found.");
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(plan));
    }

    [HttpGet]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await queryService.Handle(new GetAllPlansQuery(), cancellationToken);
        return Ok(plans.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity));
    }
}
