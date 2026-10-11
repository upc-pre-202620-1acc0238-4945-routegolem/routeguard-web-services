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
[SwaggerTag("Stakeholder driver endpoints.")]
public class DriversController(
    IDriverCommandService commandService,
    IDriverQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory,
    CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateDriver(CreateDriverResource resource, CancellationToken cancellationToken)
    {
        var command = new CreateDriverCommand(resource.OrganizationId, resource.UserId, resource.FirstName,
            resource.LastName, resource.Email, resource.PhoneNumber, resource.LicenseNumber);
        var result = await commandService.Handle(command, cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            driver => CreatedAtAction(nameof(GetDriverById), new { driverId = driver.Id.Identifier },
                StakeholderResourceFromEntityAssembler.ToResourceFromEntity(driver)));
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpGet("{driverId:guid}")]
    public async Task<IActionResult> GetDriverById(Guid driverId, CancellationToken cancellationToken)
    {
        // A driver can only read their own profile.
        if (caller.IsDriver && await caller.GetDriverIdAsync(cancellationToken) != driverId) return Forbid();
        var driver = await queryService.Handle(new GetDriverByIdQuery(driverId), cancellationToken);
        if (driver is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                StakeholderError.DriverNotFound, "Driver was not found.");
        return Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(driver));
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpGet]
    public async Task<IActionResult> GetDrivers(CancellationToken cancellationToken)
    {
        var drivers = await queryService.Handle(new GetAllDriversQuery(), cancellationToken);
        // A driver only sees their own profile (the app uses this list to find the driver id of the account).
        if (caller.IsDriver)
        {
            var own = await caller.GetDriverIdAsync(cancellationToken);
            drivers = drivers.Where(d => d.Id.Identifier == own).ToList();
        }

        return Ok(drivers.Select(StakeholderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{driverId:guid}")]
    public async Task<IActionResult> UpdateDriver(Guid driverId, CreateDriverResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new UpdateDriverCommand(driverId, resource.FirstName,
            resource.LastName, resource.Email, resource.PhoneNumber, resource.LicenseNumber, true), cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            driver => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(driver)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{driverId:guid}")]
    public async Task<IActionResult> DeleteDriver(Guid driverId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteDriverCommand(driverId), cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            driver => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(driver)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{driverId:guid}/phone-number")]
    public async Task<IActionResult> UpdatePhone(Guid driverId, UpdateDriverPhoneResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new UpdateDriverPhoneCommand(driverId, resource.PhoneNumber),
            cancellationToken);
        return StakeholderActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            driver => Ok(StakeholderResourceFromEntityAssembler.ToResourceFromEntity(driver)));
    }
}
