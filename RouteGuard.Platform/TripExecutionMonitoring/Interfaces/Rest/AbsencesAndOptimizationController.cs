using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using Swashbuckle.AspNetCore.Annotations;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using ParentAggregate = RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates.Parent;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest;

public record CreateAbsenceResource(Guid? ChildId, string ChildName, Guid? RouteId, string? AbsenceDate, string Reason, string? Notes);

[ApiController]
[Route("api/v1")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Early absence reporting (US-09) and route reassignment by absences (US-28).")]
public class AbsencesAndOptimizationController(AppDbContext context, CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("absences")]
    [SwaggerOperation("Get Absence Reports", "Returns early absence reports for the organization or caller parent (US-09).")]
    public async Task<IActionResult> GetAbsences([FromQuery] string? date, CancellationToken cancellationToken)
    {
        var query = context.Set<AbsenceReport>().AsNoTracking();
        if (caller.IsParent)
        {
            var parentId = await caller.GetParentIdAsync(cancellationToken);
            if (parentId.HasValue) query = query.Where(a => a.ParentId == parentId.Value);
        }
        else if (caller.OrganizationId is { } orgId)
        {
            query = query.Where(a => a.OrganizationId == orgId);
        }

        if (!string.IsNullOrWhiteSpace(date))
            query = query.Where(a => a.AbsenceDate == date.Trim());

        var items = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.AdminOrParent)]
    [HttpPost("absences")]
    [SwaggerOperation("Report Early Absence", "Persists an early absence notice and triggers stop optimization (US-09 / US-28).")]
    public async Task<IActionResult> CreateAbsence(CreateAbsenceResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.ChildName) || string.IsNullOrWhiteSpace(resource.Reason))
            return BadRequest(new { message = "ChildName and Reason are required." });

        var parentId = caller.IsParent ? await caller.GetParentIdAsync(cancellationToken) : null;
        var parent = parentId.HasValue
            ? await context.Set<ParentAggregate>().FirstOrDefaultAsync(p => p.Id == new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.ParentId(parentId.Value), cancellationToken)
            : await context.Set<ParentAggregate>().FirstOrDefaultAsync(cancellationToken);

        var matchedChild = parent?.Children.FirstOrDefault(c =>
            (resource.ChildId.HasValue && c.Id.Identifier == resource.ChildId.Value) ||
            string.Equals(c.FullName.ToString(), resource.ChildName.Trim(), StringComparison.OrdinalIgnoreCase));

        var resolvedRouteId = resource.RouteId ?? await context.Set<RouteAggregate>()
            .Select(r => r.Id.Identifier)
            .FirstOrDefaultAsync(cancellationToken);

        var absence = new AbsenceReport
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            ParentId = parent?.Id.Identifier ?? Guid.Empty,
            ChildId = matchedChild?.Id.Identifier ?? resource.ChildId ?? Guid.Empty,
            ChildName = matchedChild?.FullName.ToString() ?? resource.ChildName.Trim(),
            RouteId = resolvedRouteId,
            AbsenceDate = string.IsNullOrWhiteSpace(resource.AbsenceDate)
                ? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")
                : resource.AbsenceDate.Trim(),
            Reason = resource.Reason.Trim(),
            Notes = resource.Notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<AbsenceReport>().AddAsync(absence, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/absences/{absence.Id}", absence);
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("routes/{routeId:guid}/optimized-stops")]
    [SwaggerOperation("Get Route Reassignment by Absences (US-28)", "Returns route stops updated for today's confirmed absences and temporary pickup changes (US-27 + US-28).")]
    public async Task<IActionResult> GetOptimizedStops(Guid routeId, [FromQuery] string? date, CancellationToken cancellationToken)
    {
        var targetDate = string.IsNullOrWhiteSpace(date)
            ? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")
            : date.Trim();

        var route = await context.Set<RouteAggregate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == new RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.RouteId(routeId), cancellationToken);

        if (route is null)
            route = await context.Set<RouteAggregate>().AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (route is null) return NotFound();

        var absences = await context.Set<AbsenceReport>()
            .AsNoTracking()
            .Where(a => a.AbsenceDate == targetDate)
            .ToListAsync(cancellationToken);

        var pickupChanges = await context.Set<PickupChangeRequest>()
            .AsNoTracking()
            .Where(p => p.TargetDate == targetDate && p.Status != "REJECTED")
            .ToListAsync(cancellationToken);

        var orderedStops = route.Stops.OrderBy(s => s.Order.Position).ToList();
        var absentChildNames = absences.Select(a => a.ChildName).Distinct().ToList();
        var optimized = new List<object>();

        int reorderedSeq = 1;
        for (int i = 0; i < orderedStops.Count; i++)
        {
            var stop = orderedStops[i];
            var isSchoolStop = stop.Order.Position == 1 || stop.Name.Contains("Colegio", StringComparison.OrdinalIgnoreCase);

            // Match stops explicitly linked by name/order to an absent student when non-school stop
            var stopAbsence = isSchoolStop
                ? null
                : absences.FirstOrDefault(a =>
                    stop.Name.Contains(a.ChildName.Split(' ')[0], StringComparison.OrdinalIgnoreCase));

            var isSkipped = stopAbsence is not null;
            var overrideReq = isSchoolStop ? null : pickupChanges.ElementAtOrDefault(i > 0 ? i - 1 : 0);

            optimized.Add(new
            {
                stopId = stop.Id.Identifier,
                originalOrder = stop.Order.Position,
                reassignedOrder = isSkipped ? (int?)null : reorderedSeq++,
                name = overrideReq is not null && overrideReq.Status == "APPROVED"
                    ? $"{stop.Name} ({overrideReq.NewAddress})"
                    : stop.Name,
                latitude = overrideReq?.Latitude ?? stop.Coordinates.Latitude,
                longitude = overrideReq?.Longitude ?? stop.Coordinates.Longitude,
                status = isSkipped
                    ? "SKIPPED_ABSENT"
                    : (overrideReq?.Status == "APPROVED" ? "TEMP_REASSIGNED" : "ACTIVE"),
                absentChildName = stopAbsence?.ChildName
            });
        }

        return Ok(new
        {
            routeId = route.Id.Identifier,
            routeName = route.Name,
            date = targetDate,
            totalOriginalStops = orderedStops.Count,
            activeStopsCount = orderedStops.Count - absences.Count(a => orderedStops.Any(s => s.Name.Contains(a.ChildName.Split(' ')[0], StringComparison.OrdinalIgnoreCase))),
            absentStudents = absentChildNames,
            pendingPickupChanges = pickupChanges.Count,
            stops = optimized
        });
    }
}
