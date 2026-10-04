using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteAggregate = RouteGuard.Platform.Fleet.Domain.Model.Aggregates.Route;
using TripAggregate = RouteGuard.Platform.Trip.Domain.Model.Aggregates.Trip;
using RouteState = RouteGuard.Platform.Fleet.Domain.Model.ValueObjects.RouteState;

using TripOrgId = RouteGuard.Platform.Trip.Domain.Model.ValueObjects.OrganizationId;
using TripRouteId = RouteGuard.Platform.Trip.Domain.Model.ValueObjects.RouteId;
using TripDriverId = RouteGuard.Platform.Trip.Domain.Model.ValueObjects.DriverId;

namespace RouteGuard.Platform.Trip.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Trip bounded context with demo trips for the seeded active route: one trip
///     already completed and one pending for the day, so drivers and parents see history and
///     upcoming activity on first sign-in.
/// </summary>
public static class TripSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Set<TripAggregate>().AnyAsync(cancellationToken)) return;

        var activeState = new RouteState(RouteState.Active);
        var route = await context.Set<RouteAggregate>().Where(r => r.State == activeState)
            .FirstOrDefaultAsync(cancellationToken);
        var driver = await context.Set<Driver>().OrderBy(d => d.Email).FirstOrDefaultAsync(cancellationToken);
        
        if (route is null || driver is null) return;
        
        var completed = new TripAggregate(
            new TripOrgId(route.OrganizationId.Identifier), 
            new TripRouteId(route.Id.Identifier), 
            new TripDriverId(driver.Id.Identifier));
        completed.Start();
        completed.Complete();
        context.Add(completed);
        
        var pending = new TripAggregate(
            new TripOrgId(route.OrganizationId.Identifier), 
            new TripRouteId(route.Id.Identifier), 
            new TripDriverId(driver.Id.Identifier));
        context.Add(pending);

        await context.SaveChangesAsync(cancellationToken);
    }
}