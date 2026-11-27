using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model.Entities;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using FleetVehicle = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Entities.Vehicle;

using FleetOrgId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.OrganizationId;
using FleetDriverId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.DriverId;
using FleetChildId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.ChildId;

namespace RouteGuard.Platform.FleetRouteManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Fleet bounded context: the standalone vehicles catalog, active Lima routes,
///     a draft route for admin testing, and vehicle maintenance/expense logs.
///     Idempotent per entity so existing seeded databases are repaired cleanly.
/// </summary>
public static class FleetSeeder
{
    public static readonly Guid MorningRouteId = Guid.Parse("f0000000-0000-0000-0000-000000000001");
    public static readonly Guid AfternoonRouteId = Guid.Parse("f0000000-0000-0000-0000-000000000002");
    public static readonly Guid DraftSurcoRouteId = Guid.Parse("f0000000-0000-0000-0000-000000000003");

    public static readonly Guid PrimaryVehicleId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid SecondaryVehicleId = Guid.Parse("a1000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await EnsureVehiclesCatalogAsync(context, cancellationToken);

        var organizationId = new FleetOrgId(IamSeeder.SeedOrganizationId);

        var drivers = await context.Set<Driver>().ToListAsync(cancellationToken);
        var primaryDriver = drivers.FirstOrDefault(d => d.Id.Identifier == StakeholderSeeder.PrimaryDriverId)
                            ?? drivers.FirstOrDefault();
        var secondaryDriver = drivers.FirstOrDefault(d => d.Id.Identifier == StakeholderSeeder.SecondaryDriverId);

        var parents = await context.Set<Parent>()
            .Include(parent => parent.Children)
            .ToListAsync(cancellationToken);

        var allChildren = parents
            .SelectMany(parent => parent.Children)
            .ToList();

        var jorgeChildren = parents
            .Where(p => p.Id.Identifier == StakeholderSeeder.JorgeParentId)
            .SelectMany(p => p.Children)
            .ToList();

        await EnsureMorningRouteAsync(context, organizationId, primaryDriver, allChildren, cancellationToken);
        await EnsureAfternoonRouteAsync(context, organizationId, secondaryDriver,
            jorgeChildren.Count > 0 ? jorgeChildren : allChildren, cancellationToken);
        await EnsureDraftSurcoRouteAsync(context, organizationId, cancellationToken);
        await EnsureOperationsCatalogAsync(context, organizationId.Identifier, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureMorningRouteAsync(AppDbContext context, FleetOrgId organizationId,
        Driver? driver, IReadOnlyCollection<Child> children, CancellationToken cancellationToken)
    {
        var routeId = new RouteId(MorningRouteId);
        var route = await FindSeedRouteAsync(context, routeId, cancellationToken);
        if (route is null)
        {
            route = new RouteAggregate(organizationId, "Ruta San Martín - Turno Mañana");
            context.Entry(route).Property(currentRoute => currentRoute.Id).CurrentValue = routeId;
            context.Add(route);
        }

        if (!NeedsMorningRouteRepair(route, children.Count)) return;

        var assignment = driver is null
            ? null
            : new Assignment(
                AssignmentId.New(),
                new FleetDriverId(driver.Id.Identifier),
                children.Select(child => new FleetChildId(child.Id.Identifier))
            );

        var state = driver is not null && children.Count > 0
            ? new RouteState(RouteState.Active)
            : RouteState.CreateDraft();

        route.ReplaceConfiguration(
            "Ruta San Martín - Turno Mañana",
            state,
            new DepartureTime("07:00"),
            new ServiceDays(["MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY"]),
            [
                new Stop("Colegio San Martin", new Coordinates(-12.0464, -77.0428), new StopOrder(1)),
                new Stop("Parque Kennedy", new Coordinates(-12.1211, -77.0297), new StopOrder(2)),
                new Stop("Plaza San Miguel", new Coordinates(-12.0776, -77.0824), new StopOrder(3))
            ],
            new FleetVehicle(new VehicleId(PrimaryVehicleId),
                organizationId, "ABC-123", "Sprinter 416", "Mercedes-Benz", 20),
            assignment);
    }

    private static async Task EnsureAfternoonRouteAsync(AppDbContext context, FleetOrgId organizationId,
        Driver? driver, IReadOnlyCollection<Child> children, CancellationToken cancellationToken)
    {
        var routeId = new RouteId(AfternoonRouteId);
        var route = await FindSeedRouteAsync(context, routeId, cancellationToken);
        if (route is null)
        {
            route = new RouteAggregate(organizationId, "Ruta Miraflores - Turno Tarde");
            context.Entry(route).Property(currentRoute => currentRoute.Id).CurrentValue = routeId;
            context.Add(route);
        }

        if (!NeedsAfternoonRouteRepair(route)) return;

        var assignment = driver is null
            ? null
            : new Assignment(
                AssignmentId.New(),
                new FleetDriverId(driver.Id.Identifier),
                children.Select(child => new FleetChildId(child.Id.Identifier))
            );

        var state = driver is not null && children.Count > 0
            ? new RouteState(RouteState.Active)
            : RouteState.CreateDraft();

        route.ReplaceConfiguration(
            "Ruta Miraflores - Turno Tarde",
            state,
            new DepartureTime("13:30"),
            new ServiceDays(["MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY"]),
            [
                new Stop("Colegio San Martin", new Coordinates(-12.0464, -77.0428), new StopOrder(1)),
                new Stop("Óvalo Gutiérrez", new Coordinates(-12.1080, -77.0505), new StopOrder(2)),
                new Stop("Parque del Amor", new Coordinates(-12.1269, -77.0365), new StopOrder(3))
            ],
            new FleetVehicle(new VehicleId(SecondaryVehicleId),
                organizationId, "DEF-456", "Hiace", "Toyota", 15),
            assignment);
    }

    private static async Task EnsureDraftSurcoRouteAsync(AppDbContext context, FleetOrgId organizationId,
        CancellationToken cancellationToken)
    {
        var routeId = new RouteId(DraftSurcoRouteId);
        var route = await FindSeedRouteAsync(context, routeId, cancellationToken);
        if (route is null)
        {
            route = new RouteAggregate(organizationId, "Ruta Surco - Turno Mañana");
            context.Entry(route).Property(currentRoute => currentRoute.Id).CurrentValue = routeId;
            context.Add(route);
        }

        if (route.GetTotalStops() >= 2) return;

        route.ReplaceConfiguration(
            "Ruta Surco - Turno Mañana",
            RouteState.CreateDraft(),
            null,
            new ServiceDays([]),
            [
                new Stop("Colegio San Martin", new Coordinates(-12.0464, -77.0428), new StopOrder(1)),
                new Stop("Óvalo Higuereta", new Coordinates(-12.1282, -77.0014), new StopOrder(2))
            ],
            null,
            null);
    }

    private static Task<RouteAggregate?> FindSeedRouteAsync(AppDbContext context, RouteId routeId,
        CancellationToken cancellationToken) =>
        context.Set<RouteAggregate>()
            .Include(route => route.Stops)
            .Include(route => route.Vehicle)
            .Include(route => route.Assignment)
            .AsSplitQuery()
            .FirstOrDefaultAsync(route => route.Id == routeId, cancellationToken);

    private static bool NeedsMorningRouteRepair(RouteAggregate route, int availableChildrenCount) =>
        route.GetTotalStops() < 2 ||
        route.Vehicle is null ||
        route.Assignment is null ||
        availableChildrenCount > 0 && route.Assignment.GetChildCount() == 0 ||
        route.ServiceDays is null ||
        !route.ServiceDays.IsValid() ||
        route.DepartureTime is null;

    private static bool NeedsAfternoonRouteRepair(RouteAggregate route) =>
        route.State.IsDraft() ||
        route.GetTotalStops() < 2 ||
        route.Vehicle is null ||
        route.Assignment is null ||
        route.ServiceDays is null ||
        !route.ServiceDays.IsValid() ||
        route.DepartureTime is null;

    private static async Task EnsureVehiclesCatalogAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var organizationId = IamSeeder.SeedOrganizationId;
        var existing = await context.Set<VehicleCatalogItem>().ToListAsync(cancellationToken);

        var primary = existing.FirstOrDefault(v => v.Id == PrimaryVehicleId || v.Plate == "ABC-123");
        if (primary is null)
        {
            await context.Set<VehicleCatalogItem>().AddAsync(new VehicleCatalogItem
            {
                Id = PrimaryVehicleId,
                OrganizationId = organizationId,
                Plate = "ABC-123",
                Brand = "Mercedes-Benz",
                Model = "Mercedes-Benz Sprinter 416",
                Capacity = 20,
                Status = "ACTIVE",
                SoatPolicy = "SOAT-2026-88412",
                SoatInsurer = "Rímac Seguros",
                SoatExpiresAt = "2026-12-15",
                SoatValid = true
            }, cancellationToken);
        }
        else if (string.IsNullOrWhiteSpace(primary.SoatPolicy) || string.IsNullOrWhiteSpace(primary.Brand))
        {
            primary.Update(primary.Plate, primary.Model, primary.Capacity, primary.Status,
                "Mercedes-Benz", "SOAT-2026-88412", "Rímac Seguros", "2026-12-15", true);
        }

        var secondary = existing.FirstOrDefault(v => v.Id == SecondaryVehicleId || v.Plate == "DEF-456");
        if (secondary is null)
        {
            await context.Set<VehicleCatalogItem>().AddAsync(new VehicleCatalogItem
            {
                Id = SecondaryVehicleId,
                OrganizationId = organizationId,
                Plate = "DEF-456",
                Brand = "Toyota",
                Model = "Toyota Hiace",
                Capacity = 15,
                Status = "ACTIVE",
                SoatPolicy = "SOAT-2026-99104",
                SoatInsurer = "Pacífico Seguros",
                SoatExpiresAt = "2027-03-20",
                SoatValid = true
            }, cancellationToken);
        }
        else if (!string.Equals(secondary.Brand, "Toyota", StringComparison.OrdinalIgnoreCase) ||
                 string.IsNullOrWhiteSpace(secondary.SoatPolicy) ||
                 secondary.SoatPolicy == "SOAT-2026-88412")
        {
            // Repair migration default values that cloned ABC-123's Mercedes-Benz / SOAT onto DEF-456.
            secondary.Update(secondary.Plate, "Toyota Hiace", secondary.Capacity, secondary.Status,
                "Toyota", "SOAT-2026-99104", "Pacífico Seguros", "2027-03-20", true);
        }
    }

    private static async Task EnsureOperationsCatalogAsync(AppDbContext context, Guid organizationId,
        CancellationToken cancellationToken)
    {
        var existingMaintenances = await context.Set<VehicleMaintenance>().ToListAsync(cancellationToken);
        var maintenances = new[]
        {
            new VehicleMaintenance
            {
                Id = Guid.Parse("72000000-0000-0000-0000-000000000001"),
                OrganizationId = organizationId,
                VehicleId = PrimaryVehicleId,
                VehiclePlate = "ABC-123",
                MaintenanceType = "Mantenimiento preventivo 10,000 km",
                Description = "Cambio de aceite sintético, filtros y revisión integral de frenos.",
                Workshop = "Divemotor San Luis",
                Cost = 480.00m,
                OdometerKm = 42500,
                PerformedAt = "2026-11-10",
                NextDueDate = "2027-02-10",
                Status = "COMPLETED",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new VehicleMaintenance
            {
                Id = Guid.Parse("72000000-0000-0000-0000-000000000002"),
                OrganizationId = organizationId,
                VehicleId = SecondaryVehicleId,
                VehiclePlate = "DEF-456",
                MaintenanceType = "Alineamiento y balanceo",
                Description = "Calibración de suspensión delantera y rotación de neumáticos.",
                Workshop = "Autocentro Surquillo",
                Cost = 220.00m,
                OdometerKm = 31800,
                PerformedAt = "2026-11-18",
                NextDueDate = "2027-03-18",
                Status = "COMPLETED",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new VehicleMaintenance
            {
                Id = Guid.Parse("72000000-0000-0000-0000-000000000003"),
                OrganizationId = organizationId,
                VehicleId = PrimaryVehicleId,
                VehiclePlate = "ABC-123",
                MaintenanceType = "Inspección técnica vehicular anual (CITV)",
                Description = "Revisión de emisiones, luces, botiquín y extintor reglamentario escolar.",
                Workshop = "Farenet San Borja",
                Cost = 145.00m,
                OdometerKm = 43120,
                PerformedAt = DateTimeOffset.UtcNow.AddDays(-3).ToString("yyyy-MM-dd"),
                NextDueDate = "2027-11-24",
                Status = "COMPLETED",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        foreach (var item in maintenances)
        {
            if (existingMaintenances.All(m => m.Id != item.Id))
                await context.Set<VehicleMaintenance>().AddAsync(item, cancellationToken);
        }

        var existingExpenses = await context.Set<OperationalExpense>().ToListAsync(cancellationToken);
        var expenses = new[]
        {
            new OperationalExpense
            {
                Id = Guid.Parse("73000000-0000-0000-0000-000000000001"),
                OrganizationId = organizationId,
                VehicleId = PrimaryVehicleId,
                VehiclePlate = "ABC-123",
                DriverId = StakeholderSeeder.PrimaryDriverId,
                DriverName = "Carlos Ramirez",
                Category = "FUEL",
                Description = "Abastecimiento Diésel DB5 S-50 (Estación Primax Javier Prado)",
                Amount = 165.50m,
                ExpenseDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                CreatedAt = DateTimeOffset.UtcNow
            },
            new OperationalExpense
            {
                Id = Guid.Parse("73000000-0000-0000-0000-000000000002"),
                OrganizationId = organizationId,
                VehicleId = PrimaryVehicleId,
                VehiclePlate = "ABC-123",
                DriverId = StakeholderSeeder.PrimaryDriverId,
                DriverName = "Carlos Ramirez",
                Category = "TOLL",
                Description = "Peaje Vía Expresa Línea Amarilla (ida y vuelta)",
                Amount = 13.20m,
                ExpenseDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                CreatedAt = DateTimeOffset.UtcNow
            },
            new OperationalExpense
            {
                Id = Guid.Parse("73000000-0000-0000-0000-000000000003"),
                OrganizationId = organizationId,
                VehicleId = SecondaryVehicleId,
                VehiclePlate = "DEF-456",
                DriverId = StakeholderSeeder.SecondaryDriverId,
                DriverName = "Luis Torres",
                Category = "FUEL",
                Description = "Abastecimiento Gasolina Regular 90 (Repsol Av. Angamos)",
                Amount = 142.00m,
                ExpenseDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        foreach (var expense in expenses)
        {
            if (existingExpenses.All(e => e.Id != expense.Id))
                await context.Set<OperationalExpense>().AddAsync(expense, cancellationToken);
        }
    }
}