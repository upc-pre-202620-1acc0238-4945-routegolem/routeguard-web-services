using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;

public interface IDriverQueryService
{
    Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken);
}