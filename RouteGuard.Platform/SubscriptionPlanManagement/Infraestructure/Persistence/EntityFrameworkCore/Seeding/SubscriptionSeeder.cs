using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Infraestructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Subscription bounded context with the three commercial plan tiers.
/// </summary>
/// <remarks>
///     Runs once on startup after migrations; a no-op when plans already exist. The plan
///     identifiers are fixed so the frontend checkout can resolve them deterministically.
///     Quotas and prices mirror the plan catalog shown on the landing page.
/// </remarks>
public static class SubscriptionSeeder
{
    private static readonly Guid CompletePlanId = Guid.Parse("c0000000-0000-0000-0000-000000000003");

    /// <summary>Plan catalog (reference data): needed in every environment, including production.</summary>
    public static async Task SeedPlansAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<Plan>().ToListAsync(cancellationToken);

        EnsurePlan(context, existing, "c0000000-0000-0000-0000-000000000001", PlanTier.Basic, 2, 2, 49.99m);
        EnsurePlan(context, existing, "c0000000-0000-0000-0000-000000000002", PlanTier.Intermediate, 6, 6, 99.99m);
        EnsurePlan(context, existing, CompletePlanId.ToString(), PlanTier.Complete, 20, 20, 149.99m);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsurePlan(AppDbContext context, List<Plan> existing, string id, string tier, int maxRoutes,
        int maxDrivers, decimal price)
    {
        var guid = Guid.Parse(id);
        var current = existing.FirstOrDefault(p => p.Id.Identifier == guid || p.Tier.Value == tier);
        if (current is null)
        {
            AddPlan(context, id, tier, maxRoutes, maxDrivers, price);
        }
        else if (current.Price != price || current.GetRouteLimit() != maxRoutes || current.GetDriverLimit() != maxDrivers)
        {
            current.UpdateCatalog(maxRoutes, maxDrivers, price);
        }
    }

    /// <summary>Plan catalog plus the demo subscription of the demo organization (development only).</summary>
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedPlansAsync(context, cancellationToken);

        var seedSubId = new SubscriptionId(Guid.Parse("50000000-0000-0000-0000-000000000001"));
        var existing = await context.Set<Domain.Model.Aggregates.Subscription>()
            .FirstOrDefaultAsync(s => s.Id == seedSubId, cancellationToken);

        if (existing is null)
        {
            if (await context.Set<Domain.Model.Aggregates.Subscription>().AnyAsync(cancellationToken))
                return;

            // Active subscription for the seed organization on the COMPLETE tier.
            var subscription = new Domain.Model.Aggregates.Subscription(
                new OrganizationId(IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding.IamSeeder
                    .SeedOrganizationId),
                new PlanId(CompletePlanId),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(30));
            context.Entry(subscription).Property(s => s.Id).CurrentValue = seedSubId;
            context.Add(subscription);
            await context.SaveChangesAsync(cancellationToken);
        }
        else if (existing.IsActive() && existing.EndDate <= DateTimeOffset.UtcNow)
        {
            // Keep the seeded demo subscription active instead of expired at 0 remaining days.
            existing.Renew(DateTimeOffset.UtcNow.AddDays(30));
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static void AddPlan(AppDbContext context, string id, string tier, int maxRoutes, int maxDrivers,
        decimal price)
    {
        var plan = new Plan(new PlanTier(tier), new RouteQuota(maxRoutes), new DriverQuota(maxDrivers), price);
        context.Entry(plan).Property(p => p.Id).CurrentValue = new PlanId(Guid.Parse(id));
        context.Add(plan);
    }
}
