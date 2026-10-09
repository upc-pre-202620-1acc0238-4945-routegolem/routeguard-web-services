using Microsoft.AspNetCore.Authorization;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model.Entities;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.FleetRouteManagement.Interfaces.Rest.Resources;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;
using ChildId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.ChildId;
using DriverId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.DriverId;
using OrganizationId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.OrganizationId;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.FleetRouteManagement.Interfaces.Rest;

[ApiController]
[Route("api/v1/routes")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Fleet route compatibility endpoints.")]
public class RouteCompatibilityController(
    AppDbContext context,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{routeId:guid}")]
    [SwaggerOperation("Update Route", "Updates a route record and its current configuration.",
        OperationId = "UpdateRouteCompatibility")]
    public async Task<IActionResult> UpdateRoute(Guid routeId, UpdateRouteResource resource,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Name))
            return CreateProblem(StatusCodes.Status400BadRequest, FleetError.InvalidRouteData);

        var route = await FindRouteAsync(routeId, cancellationToken);
        if (route is null)
            return CreateProblem(StatusCodes.Status404NotFound, FleetError.RouteNotFound);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var state = new RouteState(NormalizeState(resource.Status ?? resource.RouteState ?? route.State.Value));
        var departureTime = CreateDepartureTime(resource.ScheduledStartTime ?? resource.DepartureTime);
        var serviceDays = CreateServiceDays(resource.ServiceDays);
        var stops = CreateStops(resource.Waypoints);
        var vehicle = CreateVehicle(route.OrganizationId, resource);
        var assignment = CreateAssignment(resource);

        route.ReplaceConfiguration(
            resource.Name.Trim(),
            state,
            departureTime,
            serviceDays,
            stops,
            vehicle,
            assignment);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(BuildRouteResource(route, resource));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{routeId:guid}")]
    [SwaggerOperation("Delete Route", "Deletes a route and dependent records used by the web application.",
        OperationId = "DeleteRouteCompatibility")]
    public async Task<IActionResult> DeleteRoute(Guid routeId, CancellationToken cancellationToken)
    {
        var route = await FindRouteAsync(routeId, cancellationToken);
        if (route is null)
            return CreateProblem(StatusCodes.Status404NotFound, FleetError.RouteNotFound);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var routeValue = new RouteId(routeId);
        var trips = await context.Set<TripAggregate>()
            .Where(trip => trip.RouteId == new RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.RouteId(routeValue.Identifier))
            .ToListAsync(cancellationToken);
        var tripIds = trips.Select(trip => new RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects.TripId(trip.Id.Identifier)).ToHashSet();

        if (tripIds.Count > 0)
        {
            var notifications = await context.Set<Notification>()
                .Where(notification => tripIds.Contains(notification.TripId))
                .ToListAsync(cancellationToken);
            context.RemoveRange(notifications);
        }

        context.RemoveRange(trips);
        context.Remove(route);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return NoContent();
    }

    private Task<RouteAggregate?> FindRouteAsync(Guid routeId, CancellationToken cancellationToken) =>
        context.Set<RouteAggregate>()
            .Include(route => route.Stops)
            .Include(route => route.Vehicle)
            .Include(route => route.Assignment)
            .AsSplitQuery()
            .FirstOrDefaultAsync(route => route.Id == new RouteId(routeId), cancellationToken);

    private IActionResult CreateProblem(int statusCode, FleetError error) =>
        problemDetailsFactory.CreateProblemDetails(this, statusCode, error, error.ToString());

    private static IReadOnlyList<Stop> CreateStops(IReadOnlyList<RouteWaypointResource>? waypoints)
    {
        var source = waypoints ?? [];
        return source.Select((waypoint, index) =>
            new Stop(
                new StopId(ParseGuidOrNew(waypoint.Id)),
                string.IsNullOrWhiteSpace(waypoint.Name) ? $"Parada {index + 1}" : waypoint.Name.Trim(),
                new Coordinates(waypoint.Latitude ?? waypoint.Lat ?? 0, waypoint.Longitude ?? waypoint.Lng ?? 0),
                new StopOrder(waypoint.Order ?? index + 1))).ToList();
    }

    private static Vehicle? CreateVehicle(OrganizationId organizationId, UpdateRouteResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.VehiclePlate)) return null;

        var vehicleId = Guid.TryParse(resource.VehicleId, out var parsedVehicleId)
            ? new VehicleId(parsedVehicleId)
            : VehicleId.New();

        return new Vehicle(
            vehicleId,
            organizationId,
            resource.VehiclePlate.Trim(),
            string.IsNullOrWhiteSpace(resource.VehicleModel) ? "Vehicle" : resource.VehicleModel.Trim(),
            string.IsNullOrWhiteSpace(resource.VehicleBrand) ? "RouteGuard" : resource.VehicleBrand.Trim(),
            Math.Max(1, resource.VehicleCapacity ?? 1));
    }

    private static Assignment? CreateAssignment(UpdateRouteResource resource)
    {
        if (!Guid.TryParse(resource.DriverId, out var driverId)) return null;

        var children = resource.StudentIds is { Count: > 0 }
            ? resource.StudentIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(id => new ChildId(Guid.Parse(id)))
                .ToList()
            : [];

        return new Assignment(AssignmentId.New(), new DriverId(driverId), children);
    }

    private static object BuildRouteResource(RouteAggregate route, UpdateRouteResource fallback)
    {
        var serviceDays = route.ServiceDays?.GetDays().ToList();

        return new
        {
            id = route.Id.ToString(),
            organizationId = route.OrganizationId.ToString(),
            name = route.Name,
            state = route.State.Value,
            departureTime = route.DepartureTime?.ToString(),
            serviceDays,
            vehicle = string.IsNullOrWhiteSpace(fallback.VehiclePlate)
                ? null
                : new
                {
                    id = fallback.VehicleId,
                    plate = fallback.VehiclePlate,
                    model = fallback.VehicleModel,
                    brand = fallback.VehicleBrand ?? "RouteGuard",
                    capacity = fallback.VehicleCapacity ?? 1
                },
            assignment = string.IsNullOrWhiteSpace(fallback.DriverId)
                ? null
                : new
                {
                    id = Guid.NewGuid().ToString(),
                    driverId = fallback.DriverId,
                    childIds = fallback.StudentIds ?? []
                },
            stops = (fallback.Waypoints ?? []).Select((waypoint, index) => new
            {
                id = waypoint.Id ?? Guid.NewGuid().ToString(),
                name = waypoint.Name ?? $"Parada {index + 1}",
                latitude = waypoint.Latitude ?? waypoint.Lat ?? 0,
                longitude = waypoint.Longitude ?? waypoint.Lng ?? 0,
                order = waypoint.Order ?? index + 1
            })
        };
    }

    private static string NormalizeState(string? state)
    {
        var normalized = string.IsNullOrWhiteSpace(state) ? RouteState.Draft : state.Trim().ToUpperInvariant();
        return normalized is RouteState.Active or RouteState.Inactive or RouteState.Draft ? normalized : RouteState.Draft;
    }

    private static DepartureTime? CreateDepartureTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return new DepartureTime(value.Trim());
    }

    private static ServiceDays CreateServiceDays(IReadOnlyList<string>? serviceDays) =>
        serviceDays is { Count: > 0 }
            ? new ServiceDays(serviceDays)
            : new ServiceDays(["MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY"]);

    private static Guid ParseGuidOrNew(string? value) =>
        Guid.TryParse(value, out var id) ? id : Guid.NewGuid();
}
