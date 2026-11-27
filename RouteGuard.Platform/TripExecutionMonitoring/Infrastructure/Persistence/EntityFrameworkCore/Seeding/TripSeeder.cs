using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.FleetRouteManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Entities;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;
using RouteState = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.RouteState;

using TripOrgId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.OrganizationId;
using TripRouteId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.RouteId;
using TripDriverId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.DriverId;
using TripChildId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.ChildId;

namespace RouteGuard.Platform.TripExecutionMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Trip bounded context with completed, in-progress and pending demo trips,
///     realistic Lima GPS breadcrumbs, waypoints, attendances, ratings, and pickup/absence records.
///     Also repairs out-of-Lima emulator GPS coordinates (e.g., Mountain View / US defaults)
///     on active Lima trips so vehicles always appear along their Lima school routes.
/// </summary>
public static class TripSeeder
{
    public static readonly Guid CompletedMorningTripId = Guid.Parse("80000000-0000-0000-0000-000000000001");
    public static readonly Guid ActiveMorningTripId = Guid.Parse("80000000-0000-0000-0000-000000000002");
    public static readonly Guid ActiveAfternoonTripId = Guid.Parse("80000000-0000-0000-0000-000000000003");
    public static readonly Guid PendingMorningTripId = Guid.Parse("80000000-0000-0000-0000-000000000004");

    /// <summary>Returns true when a coordinate lies inside Greater Lima, Peru.</summary>
    public static bool IsWithinLimaMetro(double latitude, double longitude) =>
        latitude is >= -12.50 and <= -11.55 &&
        longitude is >= -77.30 and <= -76.65;

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var activeState = new RouteState(RouteState.Active);
        var routes = await context.Set<RouteAggregate>()
            .Include(r => r.Stops)
            .Include(r => r.Assignment)
            .AsSplitQuery()
            .Where(r => r.State == activeState)
            .ToListAsync(cancellationToken);

        var morningRoute = routes.FirstOrDefault(r => r.Id.Identifier == FleetSeeder.MorningRouteId)
                           ?? routes.FirstOrDefault();
        var afternoonRoute = routes.FirstOrDefault(r => r.Id.Identifier == FleetSeeder.AfternoonRouteId);
        if (morningRoute is null) return;

        var drivers = await context.Set<Driver>().ToListAsync(cancellationToken);
        var primaryDriver = drivers.FirstOrDefault(d => d.Id.Identifier == StakeholderSeeder.PrimaryDriverId)
                            ?? drivers.FirstOrDefault();
        var secondaryDriver = drivers.FirstOrDefault(d => d.Id.Identifier == StakeholderSeeder.SecondaryDriverId);
        if (primaryDriver is null) return;

        var existingTrips = await context.Set<TripAggregate>()
            .Include(t => t.Attendances)
            .Include(t => t.Incidents)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        // 1. Ensure at least one completed morning trip with attendance records.
        var completedTrip = existingTrips.FirstOrDefault(t => t.State.IsCompleted());
        if (completedTrip is null)
        {
            completedTrip = new TripAggregate(
                new TripOrgId(morningRoute.OrganizationId.Identifier),
                new TripRouteId(morningRoute.Id.Identifier),
                new TripDriverId(primaryDriver.Id.Identifier));
            context.Entry(completedTrip).Property(t => t.Id).CurrentValue = new TripId(CompletedMorningTripId);
            completedTrip.Start();
            foreach (var childId in morningRoute.Assignment?.ChildIds ?? [])
                completedTrip.SetBoardingStatus(new TripChildId(childId), new BoardingState(BoardingState.Boarded));
            completedTrip.Complete();
            context.Entry(completedTrip).Property(t => t.StartTime).CurrentValue = DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(-45);
            context.Entry(completedTrip).Property(t => t.EndTime).CurrentValue = DateTimeOffset.UtcNow.AddDays(-1);
            context.Add(completedTrip);
            existingTrips.Add(completedTrip);
        }

        // 2. Ensure one active (IN_PROGRESS) trip for MorningRoute (Carlos Ramirez, ABC-123).
        var activeMorningTrip = existingTrips
            .Where(t => t.IsInProgress() && t.RouteId.Identifier == morningRoute.Id.Identifier)
            .OrderByDescending(t => t.StartTime)
            .FirstOrDefault();

        if (activeMorningTrip is null)
        {
            activeMorningTrip = new TripAggregate(
                new TripOrgId(morningRoute.OrganizationId.Identifier),
                new TripRouteId(morningRoute.Id.Identifier),
                new TripDriverId(primaryDriver.Id.Identifier));
            context.Entry(activeMorningTrip).Property(t => t.Id).CurrentValue = new TripId(ActiveMorningTripId);
            activeMorningTrip.Start();
            context.Entry(activeMorningTrip).Property(t => t.StartTime).CurrentValue = DateTimeOffset.UtcNow.AddMinutes(-14);
            context.Add(activeMorningTrip);
            existingTrips.Add(activeMorningTrip);
        }

        // 3. Ensure one active (IN_PROGRESS) trip for AfternoonRoute (Luis Torres, DEF-456) so Live Fleet shows 2 buses.
        TripAggregate? activeAfternoonTrip = null;
        if (afternoonRoute is not null && secondaryDriver is not null)
        {
            activeAfternoonTrip = existingTrips
                .Where(t => t.IsInProgress() && t.RouteId.Identifier == afternoonRoute.Id.Identifier)
                .OrderByDescending(t => t.StartTime)
                .FirstOrDefault();

            if (activeAfternoonTrip is null)
            {
                activeAfternoonTrip = new TripAggregate(
                    new TripOrgId(afternoonRoute.OrganizationId.Identifier),
                    new TripRouteId(afternoonRoute.Id.Identifier),
                    new TripDriverId(secondaryDriver.Id.Identifier));
                context.Entry(activeAfternoonTrip).Property(t => t.Id).CurrentValue = new TripId(ActiveAfternoonTripId);
                activeAfternoonTrip.Start();
                context.Entry(activeAfternoonTrip).Property(t => t.StartTime).CurrentValue = DateTimeOffset.UtcNow.AddMinutes(-9);
                context.Add(activeAfternoonTrip);
                existingTrips.Add(activeAfternoonTrip);
            }
        }

        // 4. Ensure at least one PENDING trip for upcoming schedule.
        if (existingTrips.All(t => !t.State.IsPending()))
        {
            var pending = new TripAggregate(
                new TripOrgId(morningRoute.OrganizationId.Identifier),
                new TripRouteId(morningRoute.Id.Identifier),
                new TripDriverId(primaryDriver.Id.Identifier));
            context.Entry(pending).Property(t => t.Id).CurrentValue = new TripId(PendingMorningTripId);
            context.Add(pending);
            existingTrips.Add(pending);
        }

        await context.SaveChangesAsync(cancellationToken);

        // 5. Ensure realistic attendances, waypoints, and Lima GPS points on active trips (and purge stray non-Lima emulator GPS).
        await EnsureLiveTripTelemetryAsync(context, activeMorningTrip, morningRoute, isMorning: true, cancellationToken);
        if (activeAfternoonTrip is not null && afternoonRoute is not null)
            await EnsureLiveTripTelemetryAsync(context, activeAfternoonTrip, afternoonRoute, isMorning: false, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        // 6. Seed service rating, pickup change request, and historical absence idempotently.
        await EnsureTripCompanionDataAsync(context, morningRoute, afternoonRoute, completedTrip, cancellationToken);
    }

    private static async Task EnsureLiveTripTelemetryAsync(
        AppDbContext context,
        TripAggregate trip,
        RouteAggregate route,
        bool isMorning,
        CancellationToken cancellationToken)
    {
        // Board first N-1 children on active trip when no attendance has been recorded yet.
        if (trip.Attendances.Count == 0 && route.Assignment?.ChildIds.Count > 0)
        {
            var childIds = route.Assignment.ChildIds;
            var boardCount = childIds.Count > 1 ? childIds.Count - 1 : 1;
            for (var i = 0; i < boardCount; i++)
                trip.SetBoardingStatus(new TripChildId(childIds[i]), new BoardingState(BoardingState.Boarded));
        }

        // Ensure trip waypoints exist and Stop 1 is marked visited.
        var waypoints = await context.Set<Waypoint>()
            .Where(w => w.TripId == trip.Id)
            .ToListAsync(cancellationToken);

        if (waypoints.Count == 0 && route.Stops.Count > 0)
        {
            waypoints = route.Stops
                .OrderBy(s => s.Order.Position)
                .Select(s => new Waypoint(trip.Id, s.Name, s.Order.Position, s.Coordinates.Latitude, s.Coordinates.Longitude))
                .ToList();
            context.AddRange(waypoints);
        }

        var firstStop = waypoints.OrderBy(w => w.OrderIndex).FirstOrDefault();
        if (firstStop is not null && waypoints.All(w => !w.IsVisited))
            firstStop.MarkVisited();

        // Check whether this route's stops are in Lima; if so, remove any stray non-Lima GPS records
        // (such as Android emulator default 37.422, -122.084 or New York coordinates).
        var routeInLima = route.Stops.Any(s => IsWithinLimaMetro(s.Coordinates.Latitude, s.Coordinates.Longitude));
        var existingLocations = await context.Set<LocationRecord>()
            .Where(r => r.TripId == trip.Id)
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync(cancellationToken);

        if (routeInLima)
        {
            var stray = existingLocations
                .Where(r => !IsWithinLimaMetro(r.Latitude, r.Longitude))
                .ToList();
            if (stray.Count > 0)
            {
                context.RemoveRange(stray);
                existingLocations = existingLocations
                    .Where(r => IsWithinLimaMetro(r.Latitude, r.Longitude))
                    .ToList();
            }
        }

        var latestValid = existingLocations.FirstOrDefault();
        var needsFreshPoint = latestValid is null ||
                              latestValid.RecordedAt < DateTimeOffset.UtcNow.AddHours(-2);

        if (needsFreshPoint)
        {
            var now = DateTimeOffset.UtcNow;
            var trail = isMorning
                ? new[]
                {
                    (-12.0464, -77.0428, 22.0, 96, 172.0, now.AddMinutes(-12)), // Colegio San Martin
                    (-12.0850, -77.0355, 34.5, 93, 169.0, now.AddMinutes(-6)),  // Av. Arequipa (Lince / San Isidro)
                    (-12.1155, -77.0308, 29.0, 91, 166.0, now)                  // Miraflores (acercándose a Parque Kennedy)
                }
                : new[]
                {
                    (-12.0464, -77.0428, 20.5, 89, 182.0, now.AddMinutes(-8)),  // Colegio San Martin
                    (-12.0810, -77.0465, 31.0, 86, 186.0, now.AddMinutes(-4)),  // San Isidro (Av. Salaverry / Pezet)
                    (-12.1045, -77.0485, 26.5, 84, 185.0, now)                  // Acercándose a Óvalo Gutiérrez
                };

            foreach (var (lat, lng, speed, battery, heading, recordedAt) in trail)
            {
                context.Add(new LocationRecord(null, trip.Id, lat, lng, speed, battery, heading, recordedAt));
            }
        }
    }

    private static async Task EnsureTripCompanionDataAsync(
        AppDbContext context,
        RouteAggregate morningRoute,
        RouteAggregate? afternoonRoute,
        TripAggregate completedTrip,
        CancellationToken cancellationToken)
    {
        var ratingId = Guid.Parse("77000000-0000-0000-0000-000000000001");
        if (!await context.Set<TripRating>().AnyAsync(r => r.Id == ratingId, cancellationToken))
        {
            context.Add(new TripRating
            {
                Id = ratingId,
                OrganizationId = morningRoute.OrganizationId.Identifier,
                TripId = completedTrip.Id.Identifier,
                ParentId = StakeholderSeeder.RositaParentId,
                UserId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                Stars = 5,
                Comment = "Excelente puntualidad y trato amable con los niños.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }

        var pickupChangeId = Guid.Parse("79000000-0000-0000-0000-000000000001");
        if (!await context.Set<PickupChangeRequest>().AnyAsync(p => p.Id == pickupChangeId, cancellationToken))
        {
            var jorgeParent = await context.Set<Parent>()
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.ParentId(StakeholderSeeder.JorgeParentId),
                    cancellationToken);
            var diego = jorgeParent?.Children.FirstOrDefault();

            context.Add(new PickupChangeRequest
            {
                Id = pickupChangeId,
                OrganizationId = morningRoute.OrganizationId.Identifier,
                ParentId = StakeholderSeeder.JorgeParentId,
                ChildId = diego?.Id.Identifier ?? Guid.Empty,
                ChildName = diego?.FullName.ToString() ?? "Diego Salas",
                RouteId = afternoonRoute?.Id.Identifier ?? morningRoute.Id.Identifier,
                TargetDate = DateTimeOffset.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                NewAddress = "Av. Pardo y Aliaga 640, San Isidro",
                Latitude = -12.1035,
                Longitude = -77.0442,
                Reason = "Consulta odontológica después del colegio",
                Status = "PENDING",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2)
            });
        }

        var absenceId = Guid.Parse("7a000000-0000-0000-0000-000000000001");
        if (!await context.Set<AbsenceReport>().AnyAsync(a => a.Id == absenceId, cancellationToken))
        {
            var jorgeParent = await context.Set<Parent>()
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.ParentId(StakeholderSeeder.JorgeParentId),
                    cancellationToken);
            var diego = jorgeParent?.Children.FirstOrDefault();

            context.Add(new AbsenceReport
            {
                Id = absenceId,
                OrganizationId = morningRoute.OrganizationId.Identifier,
                ParentId = StakeholderSeeder.JorgeParentId,
                ChildId = diego?.Id.Identifier ?? Guid.Empty,
                ChildName = diego?.FullName.ToString() ?? "Diego Salas",
                RouteId = morningRoute.Id.Identifier,
                AbsenceDate = DateTimeOffset.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
                Reason = "Cita médica",
                Notes = "Retorno regular confirmado para hoy.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}