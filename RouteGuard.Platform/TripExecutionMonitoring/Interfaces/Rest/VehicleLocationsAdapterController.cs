using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.NotificationsCommunication.Application.CommandServices;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.TripExecutionMonitoring.Application.CommandServices;
using RouteGuard.Platform.TripExecutionMonitoring.Application.QueryServices;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Queries;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest;

/// <summary>
///     Resource for legacy hardware vehicle location updates.
/// </summary>
public record LegacyVehicleLocationResource(string VehicleId, double Latitude, double Longitude);

/// <summary>
///     Hardware Adapter Endpoint to support the legacy Java tracking contract.
/// </summary>
[ApiController]
[Route("api/v1/vehicle-locations")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Hardware adapter endpoint for legacy vehicle location updates.")]
public class VehicleLocationsAdapterController(
    ITripQueryService tripQueryService,
    AppDbContext context,
    ITripCommandService tripCommandService,
    INotificationCommandService notificationCommandService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation("Record Legacy Vehicle Location", "Adapts legacy vehicle location payloads to the new Trip-centric tracking.", OperationId = "RecordLegacyVehicleLocation")]
    [SwaggerResponse(202, "The location was accepted.")]
    [SwaggerResponse(400, "The payload is invalid or no active trip exists for this vehicle.")]
    [SwaggerResponse(404, "No route found for this vehicle.")]
    public async Task<IActionResult> RecordVehicleLocation(
        [FromBody] LegacyVehicleLocationResource resource, 
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(resource.VehicleId, out var vehicleIdGuid))
        {
            return Problem(title: "Invalid VehicleId format. Must be a Guid.", statusCode: 400);
        }

        var route = await context.Set<RouteAggregate>()
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Vehicle != null && r.Vehicle.Id.Identifier == vehicleIdGuid, cancellationToken);
        
        if (route == null)
        {
            return Problem(title: "No route assigned to this vehicle.", statusCode: 404);
        }

        var trips = await tripQueryService.Handle(new GetTripsByRouteIdQuery(route.Id.Identifier), cancellationToken);
        var activeTrip = trips.FirstOrDefault(t => t.IsInProgress());

        if (activeTrip == null)
        {
            return Problem(title: "No active trip found for this vehicle's route.", statusCode: 400);
        }

        var locationResource = new LocationUpdateResource(
            Guid.NewGuid(), 
            resource.Latitude, 
            resource.Longitude, 
            0.0, 
            100, 
            0.0, 
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        var trackingController = new TripTrackingController(context, tripCommandService, notificationCommandService)
        {
            ControllerContext = this.ControllerContext
        };
        return await trackingController.RecordLocation(activeTrip.Id.Identifier, locationResource, cancellationToken);
    }
}
