using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

public interface IDriverRepository : IBaseRepository<Driver>
{
    Task<Driver?> FindByDriverIdAsync(DriverId driverId, CancellationToken cancellationToken);
}
