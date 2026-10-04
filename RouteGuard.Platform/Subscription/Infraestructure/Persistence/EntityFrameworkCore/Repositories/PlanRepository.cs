using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.Subscription.Domain.Model.Aggregates;
using RouteGuard.Platform.Subscription.Domain.Model.ValueObjects;
using RouteGuard.Platform.Subscription.Domain.Repositories;

namespace RouteGuard.Platform.Subscription.Infraestructure.Persistence.EntityFrameworkCore.Repositories;

public class PlanRepository(AppDbContext context) : BaseRepository<Plan>(context), IPlanRepository
{
    public Task<Plan?> FindByPlanIdAsync(PlanId planId, CancellationToken cancellationToken) =>
        Context.Set<Plan>().FirstOrDefaultAsync(plan => plan.Id == planId, cancellationToken);
}
