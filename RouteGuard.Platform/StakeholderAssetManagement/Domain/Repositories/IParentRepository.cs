using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

public interface IParentRepository : IBaseRepository<Parent>
{
    Task<Parent?> FindByParentIdAsync(ParentId parentId, CancellationToken cancellationToken);
}
