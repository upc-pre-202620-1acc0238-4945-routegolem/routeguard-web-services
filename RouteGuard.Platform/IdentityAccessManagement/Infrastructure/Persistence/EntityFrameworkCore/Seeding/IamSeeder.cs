using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Application.Internal.OutboundServices;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;

namespace RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Iam bounded context with the initial organization and demo user accounts.
/// </summary>
/// <remarks>
///     Idempotent per entity: checks organization and each demo user by deterministic Id/Email so
///     both fresh databases and existing databases (including Aiven PostgreSQL redeploys) receive
///     all demo accounts without duplicating or breaking existing records.
/// </remarks>
public static class IamSeeder
{
    public static readonly Guid SeedOrganizationId = Guid.Parse("a0000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext context, IHashingService hashingService,
        CancellationToken cancellationToken = default)
    {
        var orgId = new OrganizationId(SeedOrganizationId);
        var hasOrg = await context.Set<Organization>()
            .AnyAsync(o => o.Id == orgId, cancellationToken);

        if (!hasOrg)
        {
            var organization = new Organization(new OrganizationName("Transportes Escolares Lima Norte S.A.C."));
            context.Entry(organization).Property(o => o.Id).CurrentValue = orgId;
            context.Add(organization);
        }

        var existingUsers = await context.Set<User>().ToListAsync(cancellationToken);

        EnsureUser(context, existingUsers, hashingService, "b0000000-0000-0000-0000-000000000001",
            "Nickolas", "Quispe", "admin@routeguard.pe", "admin123", RoleTier.Admin);
        EnsureUser(context, existingUsers, hashingService, "b0000000-0000-0000-0000-000000000002",
            "Carlos", "Ramirez", "driver@routeguard.pe", "driver123", RoleTier.Driver);
        EnsureUser(context, existingUsers, hashingService, "b0000000-0000-0000-0000-000000000003",
            "Rosita", "Nery", "parent@routeguard.pe", "parent123", RoleTier.Parent);
        EnsureUser(context, existingUsers, hashingService, "b0000000-0000-0000-0000-000000000004",
            "Luis", "Torres", "luis.torres@routeguard.pe", "driver123", RoleTier.Driver);
        EnsureUser(context, existingUsers, hashingService, "b0000000-0000-0000-0000-000000000005",
            "Jorge", "Salas", "jorge.salas@gmail.com", "parent123", RoleTier.Parent);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureUser(AppDbContext context, List<User> existingUsers, IHashingService hashingService,
        string id, string firstName, string lastName, string email, string password, string roleTier)
    {
        var userId = new UserId(Guid.Parse(id));
        if (existingUsers.Any(u =>
                u.Id == userId ||
                string.Equals(u.Email.Value, email, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var user = new User(
            new OrganizationId(SeedOrganizationId),
            new FullName(firstName, lastName),
            new Email(email),
            new PasswordHash(hashingService.HashPassword(password)),
            new RoleTier(roleTier));
        context.Entry(user).Property(u => u.Id).CurrentValue = userId;
        context.Add(user);
        existingUsers.Add(user);
    }
}
