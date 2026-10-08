using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

namespace RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class ParentRepository(AppDbContext context) : BaseRepository<Parent>(context), IParentRepository
{
    public Task<Parent?> FindByParentIdAsync(ParentId parentId, CancellationToken cancellationToken) =>
        Context.Set<Parent>().FirstOrDefaultAsync(parent => parent.Id == parentId, cancellationToken);
}
