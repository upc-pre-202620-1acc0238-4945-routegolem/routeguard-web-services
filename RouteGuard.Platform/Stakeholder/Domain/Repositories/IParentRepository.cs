using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;

namespace RouteGuard.Platform.Stakeholder.Domain.Repositories;

public interface IParentRepository : IBaseRepository<Parent>
{
    Task<Parent?> FindByParentIdAsync(ParentId parentId, CancellationToken cancellationToken);
}
