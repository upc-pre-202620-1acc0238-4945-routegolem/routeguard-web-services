using Microsoft.AspNetCore.Authorization;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using System.Text.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.NotificationsCommunication.Application.CommandServices;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.TripExecutionMonitoring.Application.CommandServices;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Commands;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Entities;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Events;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;
using TripId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.TripId;
using TripNotificationId = RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects.TripId;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest;

/// <summary>
///     Mobile companion endpoints of the Trip bounded context: background GPS records
///     (<c>location_records</c>), offline synchronization (<c>offline_sync_batches</c>), the 500 m
///     geofence (<c>waypoints</c> + <c>geofence_alerts</c>) and the driver's panic / announcement
///     fan-out to the parents of the route.
/// </summary>
[ApiController]
[Route("api/v1/trips/{tripId:guid}")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Trip tracking, geofence, offline synchronization and driver communication endpoints.")]
public class TripTrackingController(
    AppDbContext context,
    ITripCommandService tripCommandService,
    INotificationCommandService notificationCommandService,
    IPublishEndpoint publishEndpoint,
    CallerContext caller) : ControllerBase
{
    private const string Locale = "es-PE";

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("locations")]
    public async Task<IActionResult> RecordLocation(Guid tripId, LocationUpdateResource resource,
        CancellationToken cancellationToken)
    {
        var denied = await GuardTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        await StoreLocationsAsync(tripId, [resource], cancellationToken);
        await EvaluateGeofencesAsync(tripId, resource.Latitude, resource.Longitude, cancellationToken);
        return Accepted();
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpGet("locations/latest")]
    public async Task<IActionResult> GetLatestLocation(Guid tripId, CancellationToken cancellationToken)
    {
        var denied = await GuardTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;
        var latest = await context.Set<LocationRecord>().AsNoTracking()
            .Where(r => r.TripId == new TripId(tripId))
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return latest is null
            ? NoContent()
            : Ok(new LocationUpdateResource(latest.Id, latest.Latitude, latest.Longitude, latest.SpeedKmh,
                latest.BatteryLevel, latest.Heading, latest.RecordedAt.ToUnixTimeMilliseconds()));
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("offline-sync")]
    public async Task<IActionResult> SyncOfflineRecords(Guid tripId, OfflineSyncResource resource,
        CancellationToken cancellationToken)
    {
        var denied = await GuardTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;

        var locations = (resource.Locations ?? []).OrderBy(l => l.RecordedAt).ToList();
        await StoreLocationsAsync(tripId, locations, cancellationToken);

        var syncedBoardings = 0;
        foreach (var boarding in (resource.Boardings ?? []).OrderBy(b => b.RecordedAt))
        {
            var result = await tripCommandService.Handle(
                new SetBoardingStatusCommand(tripId, boarding.ChildId, boarding.BoardingState), cancellationToken);
            if (result.IsSuccess) syncedBoardings++;
        }

        // The raw batch is kept (offline_sync_batches) so the synchronization can be audited.
        context.Add(new OfflineSyncBatch(new TripId(tripId), locations.Count + syncedBoardings,
            JsonSerializer.Serialize(resource)));
        await context.SaveChangesAsync(cancellationToken);

        // Records saved without signal may have crossed a geofence: evaluate them in order.
        foreach (var location in locations)
            await EvaluateGeofencesAsync(tripId, location.Latitude, location.Longitude, cancellationToken);
        
        // We launch the event via RabbitMQ so the Notifications module is able to notice a change
        await publishEndpoint.Publish(new OfflineSyncCompletedEvent(
            TripId: tripId,
            TotalLocationsSynced: locations.Count,
            TotalBoardingsSynced: syncedBoardings
        ), cancellationToken);

        return Ok(new { syncedLocations = locations.Count, syncedBoardings });
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("panic")]
    public async Task<IActionResult> TriggerPanic(Guid tripId, CancellationToken cancellationToken)
    {
        var denied = await GuardTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;

        var fanOut = await LoadFanOutContextAsync(tripId, cancellationToken);
        if (fanOut is null) return NotFound();

        var (trip, route, parents) = fanOut.Value;
        var (title, message) = await RenderAsync("PANIC_ALERT",
            new Dictionary<string, string> { ["route"] = route.Name },
            "Alerta de panico", $"ALERTA DE PANICO: el conductor activo una alerta en la ruta {route.Name}.",
            cancellationToken);
        await NotifyParentsAsync(trip, route, parents, "PANIC_ALERT", title, message, panic: true, null,
            cancellationToken);
        return Ok(new { notified = parents.Count });
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("broadcast")]
    public async Task<IActionResult> PostBroadcast(Guid tripId, BroadcastResource resource,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Message))
            return BadRequest(new { title = "The announcement message cannot be empty." });

        var denied = await GuardTripAsync(tripId, cancellationToken);
        if (denied is not null) return denied;

        var fanOut = await LoadFanOutContextAsync(tripId, cancellationToken);
        if (fanOut is null) return NotFound();

        var (trip, route, parents) = fanOut.Value;
        var (title, _) = await RenderAsync("ANNOUNCEMENT",
            new Dictionary<string, string> { ["route"] = route.Name, ["message"] = resource.Message.Trim() },
            "Aviso del conductor", resource.Message.Trim(), cancellationToken);
        await NotifyParentsAsync(trip, route, parents, "ANNOUNCEMENT", title, resource.Message.Trim(), panic: null,
            null, cancellationToken);
        return Ok(new { notified = parents.Count });
    }

    /// <summary>
    ///     Saves GPS points in <c>location_records</c>; a point whose id was already stored (the same
    ///     record sent live and again in an offline batch) is ignored.
    /// </summary>
    private async Task StoreLocationsAsync(Guid tripId, IReadOnlyCollection<LocationUpdateResource> locations,
        CancellationToken cancellationToken)
    {
        var existing = new HashSet<Guid>();
        if (locations.Any(l => l.Id.HasValue))
        {
            // Ids already stored for this trip (compared in memory).
            var tripKey = new TripId(tripId);
            existing = (await context.Set<LocationRecord>().AsNoTracking()
                    .Where(r => r.TripId == tripKey)
                    .Select(r => r.Id)
                    .ToListAsync(cancellationToken))
                .ToHashSet();
        }

        foreach (var location in locations)
        {
            if (location.Id.HasValue && !existing.Add(location.Id.Value)) continue;
            context.Add(new LocationRecord(location.Id, new TripId(tripId), location.Latitude, location.Longitude,
                location.SpeedKmh, location.BatteryLevel, location.Heading,
                DateTimeOffset.FromUnixTimeMilliseconds(location.RecordedAt)));
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Geofence Breached: the first time the vehicle gets within 500 m of a stop (waypoint
    ///     becomes VISITED), the parents of the route receive a notification and a geofence alert is
    ///     stored. The stop named "Colegio..." is the school arrival.
    /// </summary>
    private async Task EvaluateGeofencesAsync(Guid tripId, double latitude, double longitude,
        CancellationToken cancellationToken)
    {
        var fanOut = await LoadFanOutContextAsync(tripId, cancellationToken);
        if (fanOut is null) return;

        var (trip, route, parents) = fanOut.Value;
        if (!trip.IsInProgress()) return;

        var waypoints = await EnsureWaypointsAsync(trip, route, cancellationToken);

        foreach (var stop in route.Stops.OrderBy(s => s.Order.Position))
        {
            var waypoint = waypoints.FirstOrDefault(w => w.OrderIndex == stop.Order.Position);
            if (waypoint is null || waypoint.IsVisited) continue;

            var distance = TripLiveState.DistanceMeters(latitude, longitude,
                stop.Coordinates.Latitude, stop.Coordinates.Longitude);
            if (distance > TripLiveState.GeofenceRadiusMeters) continue;

            waypoint.MarkVisited();
            await context.SaveChangesAsync(cancellationToken);

            var isSchool = stop.Name.StartsWith("Colegio", StringComparison.OrdinalIgnoreCase);
            var values = new Dictionary<string, string>
            {
                ["stop"] = stop.Name,
                ["radius"] = TripLiveState.GeofenceRadiusMeters.ToString("0")
            };
            var type = isSchool ? "SCHOOL_ARRIVAL" : "GEOFENCE_BREACHED";
            var (title, message) = await RenderAsync(type, values,
                isSchool ? "Llegada al colegio" : "El vehiculo se acerca",
                isSchool
                    ? $"Llegada al colegio: el vehiculo llego a {stop.Name}."
                    : $"El vehiculo se acerca a la parada {stop.Name} (a menos de {values["radius"]} m).",
                cancellationToken);

            await NotifyParentsAsync(trip, route, parents, "GEOFENCE_BREACHED", title, message, panic: false,
                (isSchool ? GeofenceAlert.SchoolArrival : GeofenceAlert.Approaching, latitude, longitude),
                cancellationToken);
        }
    }

    /// <summary>The waypoints of the trip: created from the stops of the route on first use.</summary>
    private async Task<List<Waypoint>> EnsureWaypointsAsync(TripAggregate trip, RouteAggregate route,
        CancellationToken cancellationToken)
    {
        var id = trip.Id;
        var waypoints = await context.Set<Waypoint>().Where(w => w.TripId == id).ToListAsync(cancellationToken);
        if (waypoints.Count > 0) return waypoints;

        waypoints = route.Stops.OrderBy(s => s.Order.Position)
            .Select(s => new Waypoint(id, s.Name, s.Order.Position, s.Coordinates.Latitude, s.Coordinates.Longitude))
            .ToList();
        context.AddRange(waypoints);
        await context.SaveChangesAsync(cancellationToken);
        return waypoints;
    }

    /// <summary>Title and body from the stored template (es-PE); the default texts when there is none.</summary>
    private async Task<(string Title, string Body)> RenderAsync(string type, Dictionary<string, string> values,
        string defaultTitle, string defaultBody, CancellationToken cancellationToken)
    {
        var template = await context.Set<NotificationTemplate>().AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == type && t.Locale == Locale, cancellationToken);
        return template is null
            ? (defaultTitle, defaultBody)
            : (template.RenderTitle(values), template.RenderBody(values));
    }

    /// <summary>Creates, describes, flags (alert/announcement) and dispatches one notification per parent.</summary>
    private async Task NotifyParentsAsync(TripAggregate trip, RouteAggregate route, List<Parent> parents,
        string category, string title, string message, bool? panic,
        (string AlertType, double Latitude, double Longitude)? geofence, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            tripId = trip.Id.Identifier,
            routeId = route.Id.Identifier,
            category
        });

        foreach (var parent in parents)
        {
            var created = await notificationCommandService.Handle(
                new CreateNotificationCommand(trip.OrganizationId.Identifier, parent.Id.Identifier,
                    trip.Id.Identifier, category, message), cancellationToken);
            if (created.IsFailure || created.Value is null) continue;

            created.Value.Describe(title, payload);
            var id = created.Value.Id.Identifier;
            if (panic is not null)
                await notificationCommandService.Handle(new TriggerAlertCommand(id, panic.Value), cancellationToken);
            if (category == "ANNOUNCEMENT")
                await notificationCommandService.Handle(
                    new PublishAnnouncementCommand(id, route.Id.Identifier, message), cancellationToken);
            await notificationCommandService.Handle(new DispatchNotificationCommand(id), cancellationToken);

            if (geofence is { } alert)
            {
                context.Add(new GeofenceAlert(
                    new TripNotificationId(trip.Id.Identifier), 
                    new NotificationId(id),
                    alert.AlertType,
                    alert.Latitude,
                    alert.Longitude));
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>404 when the trip does not exist; 403 when a driver tries to operate a trip that is not theirs.</summary>
    private async Task<IActionResult?> GuardTripAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await FindTripAsync(tripId, cancellationToken);
        if (trip is null) return NotFound();
        if (caller.IsDriver && await caller.GetDriverIdAsync(cancellationToken) != trip.DriverId.Identifier)
            return Forbid();
        return null;
    }

    private Task<TripAggregate?> FindTripAsync(Guid tripId, CancellationToken cancellationToken) =>
        context.Set<TripAggregate>().FirstOrDefaultAsync(t => t.Id == new TripId(tripId), cancellationToken);

    /// <summary>Resolves the trip, its route and the parents of the children assigned to that route.</summary>
    private async Task<(TripAggregate Trip, RouteAggregate Route, List<Parent> Parents)?> LoadFanOutContextAsync(
        Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await FindTripAsync(tripId, cancellationToken);
        if (trip is null) return null;

        var targetRouteId = new RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.RouteId(trip.RouteId.Identifier);
        var route = await context.Set<RouteAggregate>().FirstOrDefaultAsync(r => r.Id == targetRouteId, cancellationToken);
        if (route is null) return null;

        var assignedChildIds = route.Assignment?.Children.Select(c => new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.ChildId(c.Identifier)).ToHashSet() ?? [];
        var parents = (await context.Set<Parent>().ToListAsync(cancellationToken))
            .Where(parent => parent.Children.Any(child => assignedChildIds.Contains(child.Id)))
            .ToList();

        return (trip, route, parents);
    }
}
