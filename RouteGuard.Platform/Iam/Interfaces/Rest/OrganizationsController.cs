using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.Iam.Application.CommandServices;
using RouteGuard.Platform.Iam.Application.QueryServices;
using RouteGuard.Platform.Iam.Domain.Model;
using RouteGuard.Platform.Iam.Domain.Model.Commands;
using RouteGuard.Platform.Iam.Domain.Model.Queries;
using RouteGuard.Platform.Iam.Interfaces.Rest.Resources;
using RouteGuard.Platform.Iam.Interfaces.Rest.Transform;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.Iam.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Iam organization (tenant) endpoints.")]
public class OrganizationsController(
    IOrganizationCommandService commandService,
    IOrganizationQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create an organization", OperationId = "CreateOrganization")]
    public async Task<IActionResult> CreateOrganization(CreateOrganizationResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new CreateOrganizationCommand(resource.Name), cancellationToken);
        return IamActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            organization => CreatedAtAction(nameof(GetOrganizationById),
                new { organizationId = organization.Id.Identifier },
                IamResourceFromEntityAssembler.ToResourceFromEntity(organization)));
    }

    [HttpGet("{organizationId:guid}")]
    [SwaggerOperation(Summary = "Get an organization by id", OperationId = "GetOrganizationById")]
    public async Task<IActionResult> GetOrganizationById(Guid organizationId, CancellationToken cancellationToken)
    {
        var organization = await queryService.Handle(new GetOrganizationByIdQuery(organizationId),
            cancellationToken);
        if (organization is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                IamError.OrganizationNotFound, "Organization was not found.");
        return Ok(IamResourceFromEntityAssembler.ToResourceFromEntity(organization));
    }

    [Authorize]
    [HttpPut("{organizationId:guid}")]
    [SwaggerOperation(Summary = "Update an organization", OperationId = "UpdateOrganization")]
    public async Task<IActionResult> UpdateOrganization(Guid organizationId, UpdateOrganizationResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new UpdateOrganizationCommand(organizationId, resource.Name),
            cancellationToken);
        return IamActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            organization => Ok(IamResourceFromEntityAssembler.ToResourceFromEntity(organization)));
    }
}
