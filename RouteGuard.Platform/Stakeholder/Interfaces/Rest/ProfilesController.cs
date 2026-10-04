using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.Stakeholder.Application.QueryServices;
using RouteGuard.Platform.Stakeholder.Domain.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;
using RouteGuard.Platform.Stakeholder.Interfaces.Rest.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.Stakeholder.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Stakeholder compatibility endpoint that exposes drivers and parents as profiles.")]
public class ProfilesController(
    IParentQueryService parentQueryService,
    IDriverQueryService driverQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfiles(CancellationToken cancellationToken)
    {
        var parents = await parentQueryService.Handle(new GetAllParentsQuery(), cancellationToken);
        var drivers = await driverQueryService.Handle(new GetAllDriversQuery(), cancellationToken);

        var profiles = parents.Select(ToProfileResource)
            .Concat(drivers.Select(ToProfileResource));

        return Ok(profiles);
    }

    [HttpGet("{profileId:guid}")]
    public async Task<IActionResult> GetProfileById(Guid profileId, CancellationToken cancellationToken)
    {
        var parent = await parentQueryService.Handle(new GetParentByIdQuery(profileId), cancellationToken);
        if (parent is not null) return Ok(ToProfileResource(parent));

        var driver = await driverQueryService.Handle(new GetDriverByIdQuery(profileId), cancellationToken);
        if (driver is not null) return Ok(ToProfileResource(driver));

        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
            StakeholderError.ParentNotFound, "Stakeholder profile was not found.");
    }

    private static ProfileResource ToProfileResource(Parent parent) =>
        new(parent.Id.Identifier, parent.OrganizationId.Identifier, parent.UserId.Identifier,
            parent.FullName.FirstName, parent.FullName.LastName, parent.FullName.ToString(),
            parent.PhoneNumber.Value, "parent", null, "ACTIVE");

    private static ProfileResource ToProfileResource(Driver driver) =>
        new(driver.Id.Identifier, driver.OrganizationId.Identifier, driver.UserId.Identifier,
            driver.FullName.FirstName, driver.FullName.LastName, driver.FullName.ToString(),
            driver.PhoneNumber.Value, "driver", driver.LicenseNumber.Value,
            driver.Available ? "ACTIVE" : "INACTIVE");
}
