using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.FleetRouteManagement.Interfaces.Rest;

public record CreateMaintenanceResource(
    Guid? VehicleId,
    string VehiclePlate,
    string MaintenanceType,
    string Description,
    string Workshop,
    decimal Cost,
    int OdometerKm,
    string? PerformedAt,
    string? NextDueDate);

public record CreateExpenseResource(
    Guid? VehicleId,
    string VehiclePlate,
    string Category,
    string Description,
    decimal Amount,
    string? ExpenseDate);

[ApiController]
[Route("api/v1")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Vehicle maintenance history (US-20) and operational fleet expenses (US-26).")]
public class FleetOperationsController(AppDbContext context, CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("maintenances")]
    [SwaggerOperation("Get Vehicle Maintenances (US-20)", "Returns vehicle maintenance history persisted in PostgreSQL.")]
    public async Task<IActionResult> GetMaintenances([FromQuery] Guid? vehicleId, CancellationToken cancellationToken)
    {
        var query = context.Set<VehicleMaintenance>().AsNoTracking();
        if (vehicleId.HasValue)
            query = query.Where(m => m.VehicleId == vehicleId.Value);
        else if (caller.OrganizationId is { } orgId)
            query = query.Where(m => m.OrganizationId == orgId);

        var items = await query.OrderByDescending(m => m.PerformedAt).ThenByDescending(m => m.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("maintenances")]
    [SwaggerOperation("Record Vehicle Maintenance (US-20)", "Logs a vehicle maintenance entry.")]
    public async Task<IActionResult> CreateMaintenance(CreateMaintenanceResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.MaintenanceType) || resource.Cost < 0)
            return BadRequest(new { message = "MaintenanceType and valid Cost are required." });

        var vehicle = resource.VehicleId.HasValue
            ? await context.Set<VehicleCatalogItem>().FirstOrDefaultAsync(v => v.Id == resource.VehicleId.Value, cancellationToken)
            : await context.Set<VehicleCatalogItem>().FirstOrDefaultAsync(v => v.Plate == resource.VehiclePlate, cancellationToken)
              ?? await context.Set<VehicleCatalogItem>().FirstOrDefaultAsync(cancellationToken);

        var entity = new VehicleMaintenance
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            VehicleId = vehicle?.Id ?? resource.VehicleId ?? Guid.Empty,
            VehiclePlate = !string.IsNullOrWhiteSpace(resource.VehiclePlate) ? resource.VehiclePlate.Trim() : (vehicle?.Plate ?? "ABC-123"),
            MaintenanceType = resource.MaintenanceType.Trim(),
            Description = resource.Description?.Trim() ?? string.Empty,
            Workshop = string.IsNullOrWhiteSpace(resource.Workshop) ? "Taller RouteGuard" : resource.Workshop.Trim(),
            Cost = resource.Cost,
            OdometerKm = resource.OdometerKm,
            PerformedAt = string.IsNullOrWhiteSpace(resource.PerformedAt) ? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd") : resource.PerformedAt.Trim(),
            NextDueDate = string.IsNullOrWhiteSpace(resource.NextDueDate) ? DateTimeOffset.UtcNow.AddMonths(3).ToString("yyyy-MM-dd") : resource.NextDueDate.Trim(),
            Status = "COMPLETED",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<VehicleMaintenance>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/maintenances/{entity.Id}", entity);
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpGet("expenses")]
    [SwaggerOperation("Get Operational Expenses (US-26)", "Returns fuel, toll and operating expenses.")]
    public async Task<IActionResult> GetExpenses([FromQuery] Guid? vehicleId, CancellationToken cancellationToken)
    {
        var query = context.Set<OperationalExpense>().AsNoTracking();
        if (vehicleId.HasValue)
            query = query.Where(e => e.VehicleId == vehicleId.Value);
        else if (caller.OrganizationId is { } orgId)
            query = query.Where(e => e.OrganizationId == orgId);

        var items = await query.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("expenses")]
    [SwaggerOperation("Record Operational Expense (US-26)", "Persists a fuel, toll or operational fleet expense.")]
    public async Task<IActionResult> CreateExpense(CreateExpenseResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Category) || resource.Amount <= 0)
            return BadRequest(new { message = "Category and positive Amount are required." });

        var vehicle = resource.VehicleId.HasValue
            ? await context.Set<VehicleCatalogItem>().FirstOrDefaultAsync(v => v.Id == resource.VehicleId.Value, cancellationToken)
            : await context.Set<VehicleCatalogItem>().FirstOrDefaultAsync(v => v.Plate == resource.VehiclePlate, cancellationToken)
              ?? await context.Set<VehicleCatalogItem>().FirstOrDefaultAsync(cancellationToken);

        var driverId = await caller.GetDriverIdAsync(cancellationToken);

        var entity = new OperationalExpense
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            VehicleId = vehicle?.Id ?? resource.VehicleId ?? Guid.Empty,
            VehiclePlate = !string.IsNullOrWhiteSpace(resource.VehiclePlate) ? resource.VehiclePlate.Trim() : (vehicle?.Plate ?? "ABC-123"),
            DriverId = driverId,
            DriverName = caller.Email ?? "Conductor",
            Category = resource.Category.Trim().ToUpperInvariant(),
            Description = resource.Description?.Trim() ?? string.Empty,
            Amount = resource.Amount,
            ExpenseDate = string.IsNullOrWhiteSpace(resource.ExpenseDate) ? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd") : resource.ExpenseDate.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<OperationalExpense>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/expenses/{entity.Id}", entity);
    }
}
