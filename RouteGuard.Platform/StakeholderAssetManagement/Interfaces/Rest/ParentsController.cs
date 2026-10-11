using Microsoft.AspNetCore.Authorization;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.StakeholderAssetManagement.Application.CommandServices;
using RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;
using RouteGuard.Platform.StakeholderAssetManagement.Interfaces.Rest.Resources;
using RouteGuard.Platform.StakeholderAssetManagement.Interfaces.Rest.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.StakeholderAssetManagement.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Stakeholder parent endpoints.")]
public class ParentsController(
    IParentCommandService commandService,
    IParentQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory,
    CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateParent(CreateParentResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new CreateParentCommand(resource.OrganizationId, resource.UserId,
            resource.FirstName, resource.LastName, resource.Email, resource.PhoneNumber), cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            parent => CreatedAtAction(nameof(GetParentById), new { parentId = parent.Id.Identifier },
                StakeholderResourceFromEntityAssembler.ToResourceFromEntity(parent)));
    }

    [Authorize(Roles = AppRoles.AdminOrParent)]
    [HttpGet("{parentId:guid}")]
    public async Task<IActionResult> GetParentById(Guid parentId, CancellationToken cancellationToken)
    {
        // A parent can only read their own profile.
        if (caller.IsParent && await caller.GetParentIdAsync(cancellationToken) != parentId) return Forbid();
        var parent = await queryService.Handle(new GetParentByIdQuery(parentId), cancellationToken);
        if (parent is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                StakeholderError.ParentNotFound, "Parent was not found.");
        return Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(parent));
    }

    [Authorize(Roles = AppRoles.AdminOrParent)]
    [HttpGet]
    public async Task<IActionResult> GetParents(CancellationToken cancellationToken)
    {
        var parents = await queryService.Handle(new GetAllParentsQuery(), cancellationToken);
        // A parent only sees their own profile (the app uses this list to find the parent id of the account).
        if (caller.IsParent)
        {
            var own = await caller.GetParentIdAsync(cancellationToken);
            parents = parents.Where(p => p.Id.Identifier == own).ToList();
        }

        return Ok(parents.Select(StakeholderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{parentId:guid}")]
    public async Task<IActionResult> UpdateParent(Guid parentId, CreateParentResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new UpdateParentCommand(parentId, resource.FirstName,
            resource.LastName, resource.Email, resource.PhoneNumber), cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            parent => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(parent)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{parentId:guid}")]
    public async Task<IActionResult> DeleteParent(Guid parentId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteParentCommand(parentId), cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            parent => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(parent)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{parentId:guid}/children")]
    public async Task<IActionResult> AddChild(Guid parentId, AddChildResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new AddChildToParentCommand(parentId, resource.FirstName,
            resource.LastName, resource.Age), cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            parent => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(parent)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{parentId:guid}/children/{childId:guid}")]
    public async Task<IActionResult> RemoveChild(Guid parentId, Guid childId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new RemoveChildFromParentCommand(parentId, childId),
            cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            parent => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(parent)));
    }
}
