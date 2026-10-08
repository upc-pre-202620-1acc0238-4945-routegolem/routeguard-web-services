using RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.Internal.QueryServices;

public class DriverQueryService(IDriverRepository driverRepository) : IDriverQueryService
{
    public Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken) =>
        driverRepository.FindByDriverIdAsync(new DriverId(query.DriverId), cancellationToken);

    public async Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken) =>
        await driverRepository.ListAsync(cancellationToken);
}