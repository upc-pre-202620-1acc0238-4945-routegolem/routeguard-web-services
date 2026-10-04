using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;

namespace RouteGuard.Platform.Stakeholder.Application.QueryServices;

public interface IDriverQueryService
{
    Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken);
}