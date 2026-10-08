using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;

public interface IParentQueryService
{
    Task<Parent?> Handle(GetParentByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Parent>> Handle(GetAllParentsQuery query, CancellationToken cancellationToken);
}