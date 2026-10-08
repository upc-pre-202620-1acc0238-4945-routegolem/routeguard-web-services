using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;

namespace RouteGuard.Platform.SubscriptionPlanManagement.Infraestructure.Persistence.EntityFrameworkCore.Repositories;

public class PlanRepository(AppDbContext context) : BaseRepository<Plan>(context), IPlanRepository
{
    public Task<Plan?> FindByPlanIdAsync(PlanId planId, CancellationToken cancellationToken) =>
        Context.Set<Plan>().FirstOrDefaultAsync(plan => plan.Id == planId, cancellationToken);
}
