using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Entities;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest;

/// <summary>
///     Read side used by the maps of the mobile app: the live fleet for the administrator and the
///     trip that carries the children of a parent.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Live trip monitoring for the administrator and the parents.")]
public class LiveTrackingController(AppDbContext context) : ControllerBase
{
    /// <summary>All trips in progress with the last known position of their vehicle.</summary>
    [HttpGet("trips/live")]
    public async Task<IActionResult> GetLiveTrips(CancellationToken cancellationToken)
    {
        var trips = (await context.Set<TripAggregate>().ToListAsync(cancellationToken))
            .Where(t => t.IsInProgress())
            .ToList();

        var routes = await context.Set<RouteAggregate>().ToListAsync(cancellationToken);
        var drivers = await context.Set<Driver>().ToListAsync(cancellationToken);
        var live = await LoadLiveDataAsync(trips.Select(t => t.Id), cancellationToken);

        var result = trips
            .Select(trip =>
            {
                var route = routes.FirstOrDefault(r => r.Id == new RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.RouteId(trip.RouteId.Identifier));
                return route is null ? null : Build(trip, route, drivers, children: [], live);
            })
            .Where(live => live is not null)
            .ToList();

        return Ok(result);
    }

    /// <summary>The trip in progress that carries the children of the parent, or 204 when there is none.</summary>
    [HttpGet("parents/{parentId:guid}/active-trip")]
    public async Task<IActionResult> GetParentActiveTrip(Guid parentId, CancellationToken cancellationToken)
    {
        var parent = await context.Set<Parent>()
            .FirstOrDefaultAsync(p => p.Id == new ParentId(parentId), cancellationToken);
        if (parent is null) return NotFound();

        var childIds = parent.Children.Select(c => new RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.ChildId(c.Id.Identifier)).ToHashSet();
        var routes = (await context.Set<RouteAggregate>().ToListAsync(cancellationToken))
            .Where(r => (r.Assignment?.Children ?? []).Any(childIds.Contains))
            .ToList();
        if (routes.Count == 0) return NoContent();

        var trips = (await context.Set<TripAggregate>().ToListAsync(cancellationToken))
            .Where(t => t.IsInProgress() && routes.Any(r => r.Id == new RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.RouteId(t.RouteId.Identifier)))
            .OrderByDescending(t => t.StartTime)
            .ToList();
        var trip = trips.FirstOrDefault();
        if (trip is null) return NoContent();

        var route = routes.First(r => r.Id == new RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.RouteId(trip.RouteId.Identifier));
        var drivers = await context.Set<Driver>().ToListAsync(cancellationToken);

        // Only the children of this parent that ride in this route.
        var assigned = (route.Assignment?.Children.Select(c => new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.ChildId(c.Identifier)) ?? []).ToHashSet();
        var ownChildren = parent.Children.Where(c => assigned.Contains(c.Id)).ToList();

        var live = await LoadLiveDataAsync([trip.Id], cancellationToken);
        return Ok(Build(trip, route, drivers, ownChildren, live));
    }

    /// <summary>Last GPS point (<c>location_records</c>) and visited stops (<c>waypoints</c>) of each trip.</summary>
    private async Task<LiveData> LoadLiveDataAsync(IEnumerable<TripId> tripIds, CancellationToken cancellationToken)
    {
        var ids = tripIds.ToList();
        var latest = new Dictionary<Guid, LocationRecord>();
        foreach (var id in ids)
        {
            var record = await context.Set<LocationRecord>().AsNoTracking()
                .Where(r => r.TripId == id)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (record is not null) latest[id.Identifier] = record;
        }

        var visited = new Dictionary<Guid, HashSet<int>>();
        foreach (var id in ids)
        {
            var orders = await context.Set<Waypoint>().AsNoTracking()
                .Where(w => w.TripId == id && w.Status == Waypoint.Visited)
                .Select(w => w.OrderIndex)
                .ToListAsync(cancellationToken);
            if (orders.Count > 0) visited[id.Identifier] = orders.ToHashSet();
        }

        return new LiveData(latest, visited);
    }

    private sealed record LiveData(Dictionary<Guid, LocationRecord> Latest, Dictionary<Guid, HashSet<int>> Visited);

    private static LiveTripResource Build(TripAggregate trip, RouteAggregate route, List<Driver> drivers,
        IReadOnlyCollection<Child> children, LiveData live)
    {
        var tripId = trip.Id.Identifier;
        live.Latest.TryGetValue(tripId, out var latest);
        live.Visited.TryGetValue(tripId, out var visited);
        var attendances = trip.Attendances.ToList();

        var stops = route.Stops
            .OrderBy(s => s.Order.Position)
            .Select(s => new LiveStopResource(s.Id.Identifier, s.Name, s.Coordinates.Latitude,
                s.Coordinates.Longitude, s.Order.Position, visited?.Contains(s.Order.Position) ?? false))
            .ToList();

        var liveChildren = children
            .Select(c => new LiveChildResource(c.Id.Identifier, c.FullName.ToString(),
                attendances.FirstOrDefault(a => a.ChildId == new RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.ChildId(c.Id.Identifier))?.BoardingState.Value
                ?? "MISSING"))
            .ToList();

        var driver = drivers.FirstOrDefault(d => d.Id == new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.DriverId(trip.DriverId.Identifier));

        return new LiveTripResource(
            tripId,
            route.Id.Identifier,
            route.Name,
            driver?.FullName.ToString() ?? "Conductor",
            trip.StartTime,
            attendances.Count(a => a.BoardingState.IsBoarded()),
            route.Assignment?.ChildIds.Count ?? 0,
            latest is null ? null : new LiveLocationResource(latest.Latitude, latest.Longitude, latest.SpeedKmh,
                latest.RecordedAt.ToUnixTimeMilliseconds()),
            stops,
            liveChildren);
    }
}
