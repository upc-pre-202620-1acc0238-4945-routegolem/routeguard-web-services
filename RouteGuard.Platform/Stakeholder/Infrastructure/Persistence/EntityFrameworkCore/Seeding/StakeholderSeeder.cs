using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Commands;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Stakeholder.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Stakeholder bounded context with demo drivers, parents and children for the
///     seed organization. The first driver and parent are linked to the seeded IAM accounts
///     (driver@routeguard.pe / parent@routeguard.pe) through their shared UserId.
/// </summary>
public static class StakeholderSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Set<Driver>().AnyAsync(cancellationToken) ||
            await context.Set<Parent>().AnyAsync(cancellationToken)) return;

        var organizationId = IamSeeder.SeedOrganizationId;

        AddDriver(context, "d0000000-0000-0000-0000-000000000001", organizationId,
            "b0000000-0000-0000-0000-000000000002", "Carlos", "Ramirez", "driver@routeguard.pe",
            "987654321", "Q12345678");
        AddDriver(context, "d0000000-0000-0000-0000-000000000002", organizationId,
            "b0000000-0000-0000-0000-000000000004", "Luis", "Torres", "luis.torres@routeguard.pe",
            "987112233", "Q87654321");

        var rosita = new Parent(new CreateParentCommand(organizationId,
            Guid.Parse("b0000000-0000-0000-0000-000000000003"), "Rosita", "Nery", "parent@routeguard.pe",
            "999888777"));
        rosita.AddChild(new Child(new FullName("Mateo", "Nery"), 7));
        rosita.AddChild(new Child(new FullName("Luciana", "Nery"), 9));
        AddParent(context, "e0000000-0000-0000-0000-000000000001", rosita);

        var jorge = new Parent(new CreateParentCommand(organizationId,
            Guid.Parse("b0000000-0000-0000-0000-000000000005"), "Jorge", "Salas", "jorge.salas@gmail.com",
            "998877665"));
        jorge.AddChild(new Child(new FullName("Diego", "Salas"), 8));
        AddParent(context, "e0000000-0000-0000-0000-000000000002", jorge);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void AddDriver(AppDbContext context, string id, Guid organizationId, string userId,
        string firstName, string lastName, string email, string phone, string license)
    {
        var driver = new Driver(new CreateDriverCommand(organizationId, Guid.Parse(userId), firstName, lastName,
            email, phone, license));
        context.Entry(driver).Property(d => d.Id).CurrentValue = new DriverId(Guid.Parse(id));
        context.Add(driver);
    }

    private static void AddParent(AppDbContext context, string id, Parent parent)
    {
        context.Entry(parent).Property(p => p.Id).CurrentValue = new ParentId(Guid.Parse(id));
        context.Add(parent);
    }
}
