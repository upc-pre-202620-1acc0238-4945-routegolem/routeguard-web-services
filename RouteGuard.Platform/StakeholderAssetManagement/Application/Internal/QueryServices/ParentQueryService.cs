using RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.Internal.QueryServices;

public class ParentQueryService(IParentRepository parentRepository) : IParentQueryService
{
    public Task<Parent?> Handle(GetParentByIdQuery query, CancellationToken cancellationToken) =>
        parentRepository.FindByParentIdAsync(new ParentId(query.ParentId), cancellationToken);

    public async Task<IEnumerable<Parent>> Handle(GetAllParentsQuery query, CancellationToken cancellationToken) =>
        await parentRepository.ListAsync(cancellationToken);
}