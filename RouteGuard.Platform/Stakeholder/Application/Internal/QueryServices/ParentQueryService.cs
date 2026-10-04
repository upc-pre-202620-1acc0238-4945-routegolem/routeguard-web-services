using RouteGuard.Platform.Stakeholder.Application.QueryServices;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Application.Internal.QueryServices;

public class ParentQueryService(IParentRepository parentRepository) : IParentQueryService
{
    public Task<Parent?> Handle(GetParentByIdQuery query, CancellationToken cancellationToken) =>
        parentRepository.FindByParentIdAsync(new ParentId(query.ParentId), cancellationToken);

    public async Task<IEnumerable<Parent>> Handle(GetAllParentsQuery query, CancellationToken cancellationToken) =>
        await parentRepository.ListAsync(cancellationToken);
}