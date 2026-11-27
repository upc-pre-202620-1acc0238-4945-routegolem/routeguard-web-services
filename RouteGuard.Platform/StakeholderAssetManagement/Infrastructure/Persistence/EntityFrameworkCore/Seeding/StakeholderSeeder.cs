using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Stakeholder bounded context with demo drivers, parents, children, student groups,
///     and authorized caregivers for the seed organization. Idempotent per entity so existing
///     databases are repaired cleanly without duplicating data.
/// </summary>
public static class StakeholderSeeder
{
    public static readonly Guid PrimaryDriverId = Guid.Parse("d0000000-0000-0000-0000-000000000001");
    public static readonly Guid SecondaryDriverId = Guid.Parse("d0000000-0000-0000-0000-000000000002");
    public static readonly Guid RositaParentId = Guid.Parse("e0000000-0000-0000-0000-000000000001");
    public static readonly Guid JorgeParentId = Guid.Parse("e0000000-0000-0000-0000-000000000002");

    private static readonly Guid MorningGroupId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private static readonly Guid AfternoonGroupId = Guid.Parse("60000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var organizationId = IamSeeder.SeedOrganizationId;

        var existingDrivers = await context.Set<Driver>().ToListAsync(cancellationToken);
        EnsureDriver(context, existingDrivers, PrimaryDriverId, organizationId,
            "b0000000-0000-0000-0000-000000000002", "Carlos", "Ramirez", "driver@routeguard.pe",
            "987654321", "Q12345678", "A-IIIa", "2027-08-30");
        EnsureDriver(context, existingDrivers, SecondaryDriverId, organizationId,
            "b0000000-0000-0000-0000-000000000004", "Luis", "Torres", "luis.torres@routeguard.pe",
            "987112233", "Q87654321", "A-IIIa", "2028-02-15");

        var existingParents = await context.Set<Parent>()
            .Include(p => p.Children)
            .ToListAsync(cancellationToken);

        var rosita = EnsureParent(context, existingParents, RositaParentId, organizationId,
            "b0000000-0000-0000-0000-000000000003", "Rosita", "Nery", "parent@routeguard.pe",
            "999888777",
            [("Mateo", "Nery", 7), ("Luciana", "Nery", 9)]);

        var jorge = EnsureParent(context, existingParents, JorgeParentId, organizationId,
            "b0000000-0000-0000-0000-000000000005", "Jorge", "Salas", "jorge.salas@gmail.com",
            "998877665",
            [("Diego", "Salas", 8)]);

        await context.SaveChangesAsync(cancellationToken);

        await EnsureStudentGroupsSeededAsync(context, organizationId, rosita, jorge, cancellationToken);
    }

    public static async Task EnsureCaregiversSeededAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var parents = await context.Set<Parent>().ToListAsync(cancellationToken);
        var rositaParent = parents.FirstOrDefault(p => p.Id.Identifier == RositaParentId)
                           ?? parents.FirstOrDefault(p =>
                               string.Equals(p.Email.Value, "parent@routeguard.pe", StringComparison.OrdinalIgnoreCase));
        if (rositaParent is null) return;

        var jorgeParent = parents.FirstOrDefault(p => p.Id.Identifier == JorgeParentId)
                          ?? parents.FirstOrDefault(p =>
                              string.Equals(p.Email.Value, "jorge.salas@gmail.com", StringComparison.OrdinalIgnoreCase));

        var existing = await context.Set<AuthorizedCaregiver>().ToListAsync(cancellationToken);

        var cg1Id = Guid.Parse("71000000-0000-0000-0000-000000000001");
        var cg1 = existing.FirstOrDefault(c => c.Id == cg1Id);
        if (cg1 is null)
        {
            context.Add(new AuthorizedCaregiver
            {
                Id = cg1Id,
                OrganizationId = rositaParent.OrganizationId.Identifier,
                ParentId = rositaParent.Id.Identifier,
                FullName = "Carmen López de Aljovín",
                Relationship = "Abuela / Cuidadora principal",
                PhoneNumber = "987-123-456",
                DocumentNumber = "08491230",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else if (cg1.ParentId != rositaParent.Id.Identifier)
        {
            // Repair earlier seed where alphabetical Email sort attached Rosita's caregivers to Jorge.
            cg1.ParentId = rositaParent.Id.Identifier;
        }

        var cg2Id = Guid.Parse("71000000-0000-0000-0000-000000000002");
        var cg2 = existing.FirstOrDefault(c => c.Id == cg2Id);
        if (cg2 is null)
        {
            context.Add(new AuthorizedCaregiver
            {
                Id = cg2Id,
                OrganizationId = rositaParent.OrganizationId.Identifier,
                ParentId = rositaParent.Id.Identifier,
                FullName = "Laura Gómez",
                Relationship = "Madre / Apoderada alterna",
                PhoneNumber = "999-888-777",
                DocumentNumber = "42105598",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else if (cg2.ParentId != rositaParent.Id.Identifier)
        {
            cg2.ParentId = rositaParent.Id.Identifier;
        }

        var cg3Id = Guid.Parse("71000000-0000-0000-0000-000000000003");
        if (jorgeParent is not null && existing.All(c => c.Id != cg3Id))
        {
            context.Add(new AuthorizedCaregiver
            {
                Id = cg3Id,
                OrganizationId = jorgeParent.OrganizationId.Identifier,
                ParentId = jorgeParent.Id.Identifier,
                FullName = "Elena Salas",
                Relationship = "Tía / Apoderada alterna",
                PhoneNumber = "991-223-344",
                DocumentNumber = "41208877",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureStudentGroupsSeededAsync(AppDbContext context, Guid organizationId,
        Parent rosita, Parent jorge, CancellationToken cancellationToken)
    {
        var existingGroups = await context.Set<StudentGroup>().ToListAsync(cancellationToken);
        var org = new OrganizationId(organizationId);

        if (existingGroups.All(g => g.Id.Identifier != MorningGroupId))
        {
            var morningGroup = new StudentGroup(org, "Grupo San Martín - Primaria");
            context.Entry(morningGroup).Property(g => g.Id).CurrentValue = new StudentGroupId(MorningGroupId);
            foreach (var child in rosita.Children.Concat(jorge.Children))
                morningGroup.AddChild(child.Id);
            morningGroup.Finalize();
            context.Add(morningGroup);
        }

        if (existingGroups.All(g => g.Id.Identifier != AfternoonGroupId))
        {
            var afternoonGroup = new StudentGroup(org, "Grupo Miraflores - Turno Tarde");
            context.Entry(afternoonGroup).Property(g => g.Id).CurrentValue = new StudentGroupId(AfternoonGroupId);
            foreach (var child in jorge.Children)
                afternoonGroup.AddChild(child.Id);
            if (afternoonGroup.GetChildCount() > 0)
                afternoonGroup.Finalize();
            context.Add(afternoonGroup);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureDriver(AppDbContext context, List<Driver> existing, Guid id, Guid organizationId,
        string userId, string firstName, string lastName, string email, string phone, string license,
        string licenseCategory, string licenseExpiresAt)
    {
        var driverId = new DriverId(id);
        var found = existing.FirstOrDefault(d =>
            d.Id == driverId ||
            string.Equals(d.Email.Value, email, StringComparison.OrdinalIgnoreCase));
        if (found is not null)
        {
            found.UpdateLicenseDetails(licenseCategory, licenseExpiresAt);
            return;
        }

        var driver = new Driver(new CreateDriverCommand(organizationId, Guid.Parse(userId), firstName, lastName,
            email, phone, license));
        driver.UpdateLicenseDetails(licenseCategory, licenseExpiresAt);
        context.Entry(driver).Property(d => d.Id).CurrentValue = driverId;
        context.Add(driver);
        existing.Add(driver);
    }

    private static Parent EnsureParent(AppDbContext context, List<Parent> existing, Guid id, Guid organizationId,
        string userId, string firstName, string lastName, string email, string phone,
        IReadOnlyList<(string First, string Last, int Age)> children)
    {
        var parentId = new ParentId(id);
        var found = existing.FirstOrDefault(p =>
            p.Id == parentId ||
            string.Equals(p.Email.Value, email, StringComparison.OrdinalIgnoreCase));
        if (found is not null) return found;

        var parent = new Parent(new CreateParentCommand(organizationId, Guid.Parse(userId),
            firstName, lastName, email, phone));
        foreach (var (childFirst, childLast, age) in children)
            parent.AddChild(new Child(new FullName(childFirst, childLast), age));
        context.Entry(parent).Property(p => p.Id).CurrentValue = parentId;
        context.Add(parent);
        existing.Add(parent);
        return parent;
    }
}
