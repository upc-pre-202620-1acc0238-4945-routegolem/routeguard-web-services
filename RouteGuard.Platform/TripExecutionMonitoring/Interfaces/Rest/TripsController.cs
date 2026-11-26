using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using RouteGuard.Platform.Shared.Resources.Errors;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.TripExecutionMonitoring.Application.CommandServices;
using RouteGuard.Platform.TripExecutionMonitoring.Application.QueryServices;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Commands;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Queries;
using RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest.Resources;
using RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest.Transform;
using Swashbuckle.AspNetCore.Annotations;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest;

public record RateTripResource(int Stars, string? Comment);
public record CancelTripResource(string? Reason);

/// <summary>
///     REST endpoints for the Trip Execution &amp; Monitoring bounded context.
/// </summary>
/// <remarks>
///     The controller is a thin adapter: it converts resources to commands/queries, delegates to the
///     application services, and translates the results to HTTP responses through the assemblers. It
///     contains no business logic.
/// </remarks>
/// <param name="tripCommandService">The trip command (write) service.</param>
/// <param name="tripQueryService">The trip query (read) service.</param>
/// <param name="errorLocalizer">The error message localizer.</param>
/// <param name="problemDetailsFactory">The factory used to produce RFC 7807 problem details.</param>
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Trip execution and monitoring endpoints.")]
public class TripsController(
    ITripCommandService tripCommandService,
    ITripQueryService tripQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory,
    CallerContext caller,
    AppDbContext dbContext)
    : ControllerBase
{
    /// <summary>403 when a driver tries to operate a trip that belongs to another driver (admins are not limited).</summary>
    private async Task<IActionResult?> DriverOwnsTripAsync(Guid tripId, CancellationToken cancellationToken)
    {
        if (!caller.IsDriver) return null;
        var trip = await tripQueryService.Handle(new GetTripByIdQuery(tripId), cancellationToken);
        if (trip is null) return null; // the action answers 404
        return await caller.GetDriverIdAsync(cancellationToken) == trip.DriverId.Identifier ? null : Forbid();
    }

    /// <summary>Prepares (creates) a new trip session.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost]
    [SwaggerOperation("Create Trip", "Prepares a new trip session for a route and driver.",
        OperationId = "CreateTrip")]
    [SwaggerResponse(201, "The trip was created.", typeof(TripResource))]
    [SwaggerResponse(400, "The trip could not be created.")]
    public async Task<IActionResult> CreateTrip(CreateTripResource resource, CancellationToken cancellationToken)
    {
        // A driver can only prepare trips for themselves.
        if (caller.IsDriver &&
            (!Guid.TryParse(resource.DriverId, out var requestedDriver) ||
             requestedDriver != await caller.GetDriverIdAsync(cancellationToken)))
            return Forbid();

        var command = CreateTripCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await tripCommandService.Handle(command, cancellationToken);

        return TripActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            trip => CreatedAtAction(nameof(GetTripById), new { tripId = trip.Id.Identifier },
                TripResourceFromEntityAssembler.ToResourceFromEntity(trip)));
    }
    
    /// <summary>Starts a prepared trip.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("{tripId:guid}/start")]
    [SwaggerOperation("Start Trip", "Starts a prepared trip.", OperationId = "StartTrip")]
    [SwaggerResponse(200, "The trip was started.", typeof(TripResource))]
    [SwaggerResponse(404, "The trip was not found.")]
    [SwaggerResponse(409, "The trip is not in a startable state.")]
    public async Task<IActionResult> StartTrip(Guid tripId, CancellationToken cancellationToken)
    {
        var denied = await DriverOwnsTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        var result = await tripCommandService.Handle(new StartTripCommand(tripId), cancellationToken);
        return TripActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            trip => Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip)));
    }
    
    /// <summary>Gets a trip by its unique identifier.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpGet("{tripId:guid}")]
    [SwaggerOperation("Get Trip by Id", "Gets a trip by its unique identifier.", OperationId = "GetTripById")]
    [SwaggerResponse(200, "The trip was found.", typeof(TripResource))]
    [SwaggerResponse(404, "The trip was not found.")]
    public async Task<IActionResult> GetTripById(Guid tripId, CancellationToken cancellationToken)
    {
        var denied = await DriverOwnsTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        var trip = await tripQueryService.Handle(new GetTripByIdQuery(tripId), cancellationToken);

        return TripActionResultAssembler.ToActionResultFromLookup(this, trip, errorLocalizer, problemDetailsFactory,
            found => Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }
    
    /// <summary>Gets all trips, optionally filtered by route.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpGet]
    [SwaggerOperation("Get Trips", "Gets all trips, optionally filtered by route id.", OperationId = "GetTrips")]
    [SwaggerResponse(200, "The trips were found.", typeof(IEnumerable<TripResource>))]
    public async Task<IActionResult> GetTrips([FromQuery] Guid? routeId, CancellationToken cancellationToken)
    {
        var trips = routeId is null
            ? await tripQueryService.Handle(new GetAllTripsQuery(), cancellationToken)
            : await tripQueryService.Handle(new GetTripsByRouteIdQuery(routeId.Value), cancellationToken);

        // A driver only sees their own trips.
        if (caller.IsDriver)
        {
            var own = await caller.GetDriverIdAsync(cancellationToken);
            trips = trips.Where(t => t.DriverId.Identifier == own).ToList();
        }

        return Ok(trips.Select(TripResourceFromEntityAssembler.ToResourceFromEntity));
    }
    
    /// <summary>Records a child's boarding status during a trip.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("{tripId:guid}/boarding")]
    [SwaggerOperation("Record Boarding", "Records a child's boarding status during a trip.",
        OperationId = "RecordBoarding")]
    [SwaggerResponse(200, "The boarding status was recorded.", typeof(TripResource))]
    [SwaggerResponse(404, "The trip was not found.")]
    [SwaggerResponse(409, "The trip is not in progress.")]
    public async Task<IActionResult> RecordBoarding(Guid tripId, SetBoardingStatusResource resource,
        CancellationToken cancellationToken)
    {
        var denied = await DriverOwnsTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        var command = SetBoardingStatusCommandFromResourceAssembler.ToCommandFromResource(tripId, resource);
        var result = await tripCommandService.Handle(command, cancellationToken);
        return TripActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            trip => Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip)));
    }

    /// <summary>Reports an incident during a trip.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("{tripId:guid}/incidents")]
    [SwaggerOperation("Report Incident", "Reports an incident during a trip.", OperationId = "ReportIncident")]
    [SwaggerResponse(200, "The incident was reported.", typeof(TripResource))]
    [SwaggerResponse(404, "The trip was not found.")]
    [SwaggerResponse(409, "The trip is not in progress.")]
    public async Task<IActionResult> ReportIncident(Guid tripId, ReportIncidentResource resource,
        CancellationToken cancellationToken)
    {
        var denied = await DriverOwnsTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        var command = ReportIncidentCommandFromResourceAssembler.ToCommandFromResource(tripId, resource);
        var result = await tripCommandService.Handle(command, cancellationToken);
        return TripActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            trip => Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip)));
    }    
    
    /// <summary>Completes an in-progress trip.</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("{tripId:guid}/complete")]
    [SwaggerOperation("Complete Trip", "Completes an in-progress trip.", OperationId = "CompleteTrip")]
    [SwaggerResponse(200, "The trip was completed.", typeof(TripResource))]
    [SwaggerResponse(404, "The trip was not found.")]
    [SwaggerResponse(409, "The trip is not in progress.")]
    public async Task<IActionResult> CompleteTrip(Guid tripId, CancellationToken cancellationToken)
    {
        var denied = await DriverOwnsTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        var result = await tripCommandService.Handle(new CompleteTripCommand(tripId), cancellationToken);
        return TripActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            trip => Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip)));
    }

    /// <summary>Cancels a trip for force majeure (US-21).</summary>
    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("{tripId:guid}/cancel")]
    [SwaggerOperation("Cancel Trip (US-21)", "Cancels a pending or in-progress trip due to force majeure.", OperationId = "CancelTrip")]
    public async Task<IActionResult> CancelTrip(Guid tripId, [FromBody] CancelTripResource? resource, CancellationToken cancellationToken)
    {
        var denied = await DriverOwnsTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;

        var trip = await dbContext.Set<TripAggregate>()
            .FirstOrDefaultAsync(t => t.Id == new Domain.Model.ValueObjects.TripId(tripId), cancellationToken);
        if (trip is null)
            return NotFound();

        if (!trip.State.IsCompleted() && !trip.State.IsCancelled())
        {
            if (trip.State.IsInProgress() && !string.IsNullOrWhiteSpace(resource?.Reason))
            {
                trip.ReportIncident(new Domain.Model.ValueObjects.IncidentDescription($"Cancelación: {resource.Reason.Trim()}"));
            }
            trip.Cancel();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip));
    }

    /// <summary>Gets enriched trip history for Parent, Driver or Admin (US-15).</summary>
    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("history")]
    [SwaggerOperation("Get Trip History (US-15)", "Returns completed, cancelled and active trips enriched with driver, vehicle and rating details.")]
    public async Task<IActionResult> GetTripHistory(CancellationToken cancellationToken)
    {
        var trips = (await dbContext.Set<TripAggregate>()
                .Include(t => t.Attendances)
                .Include(t => t.Incidents)
                .ToListAsync(cancellationToken))
            .OrderByDescending(t => t.EndTime ?? t.CancelledAt ?? t.StartTime ?? t.CreatedAt ?? DateTimeOffset.UtcNow)
            .ToList();

        if (caller.IsDriver)
        {
            var ownDriver = await caller.GetDriverIdAsync(cancellationToken);
            trips = trips.Where(t => t.DriverId.Identifier == ownDriver).ToList();
        }

        var routes = await dbContext.Set<RouteAggregate>().AsNoTracking().ToListAsync(cancellationToken);
        var drivers = await dbContext.Set<Driver>().AsNoTracking().ToListAsync(cancellationToken);
        var ratings = await dbContext.Set<TripRating>().AsNoTracking().ToListAsync(cancellationToken);
        var callerUserId = caller.UserId;

        var result = trips.Select(t =>
        {
            var route = routes.FirstOrDefault(r => r.Id.Identifier == t.RouteId.Identifier);
            var driver = drivers.FirstOrDefault(d => d.Id.Identifier == t.DriverId.Identifier);
            var tripRatings = ratings.Where(r => r.TripId == t.Id.Identifier).ToList();
            var myRating = callerUserId.HasValue
                ? tripRatings.FirstOrDefault(r => r.UserId == callerUserId.Value)?.Stars
                : null;
            var avgRating = tripRatings.Count > 0 ? Math.Round(tripRatings.Average(r => r.Stars), 1) : (double?)null;

            return new
            {
                tripId = t.Id.Identifier,
                routeId = t.RouteId.Identifier,
                routeName = route?.Name ?? "Ruta escolar",
                driverName = driver?.FullName.ToString() ?? "Conductor",
                vehiclePlate = route?.Vehicle?.Plate ?? "ABC-123",
                status = t.State.Value,
                boardedCount = t.Attendances.Count(a => a.BoardingState.IsBoarded()),
                totalStudents = route?.Assignment?.ChildIds.Count ?? t.Attendances.Count,
                startTime = t.StartTime,
                endTime = t.EndTime ?? t.CancelledAt,
                myRating = myRating,
                averageRating = avgRating,
                cancellationReason = t.State.IsCancelled()
                    ? t.Incidents.LastOrDefault()?.Description.Value
                    : null
            };
        });

        return Ok(result);
    }

    /// <summary>Submits or updates a service rating for a trip (US-14).</summary>
    [Authorize(Roles = AppRoles.Any)]
    [HttpPost("{tripId:guid}/rating")]
    [SwaggerOperation("Rate Trip (US-14)", "Stores or updates a 1-5 star service rating for a trip.")]
    public async Task<IActionResult> RateTrip(Guid tripId, RateTripResource resource, CancellationToken cancellationToken)
    {
        if (resource.Stars < 1 || resource.Stars > 5)
            return BadRequest(new { message = "Stars must be between 1 and 5." });

        var userId = caller.UserId ?? Guid.Empty;
        var parentId = caller.IsParent ? await caller.GetParentIdAsync(cancellationToken) : null;

        var existing = await dbContext.Set<TripRating>()
            .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == userId, cancellationToken);

        if (existing is null)
        {
            existing = new TripRating
            {
                Id = Guid.NewGuid(),
                OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
                TripId = tripId,
                ParentId = parentId,
                UserId = userId,
                Stars = resource.Stars,
                Comment = resource.Comment?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            await dbContext.Set<TripRating>().AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.Stars = resource.Stars;
            existing.Comment = resource.Comment?.Trim();
            existing.CreatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(existing);
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("{tripId:guid}/ratings")]
    [SwaggerOperation("Get Trip Ratings (US-14)", "Gets all ratings for a trip.")]
    public async Task<IActionResult> GetTripRatings(Guid tripId, CancellationToken cancellationToken)
    {
        var items = await dbContext.Set<TripRating>()
            .AsNoTracking()
            .Where(r => r.TripId == tripId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }
}