using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;

namespace RouteGuard.Platform.Stakeholder.Application.QueryServices;

public interface IParentQueryService
{
    Task<Parent?> Handle(GetParentByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Parent>> Handle(GetAllParentsQuery query, CancellationToken cancellationToken);
}