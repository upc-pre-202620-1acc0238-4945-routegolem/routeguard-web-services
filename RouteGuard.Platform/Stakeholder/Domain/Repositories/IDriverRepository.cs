using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;

namespace RouteGuard.Platform.Stakeholder.Domain.Repositories;

public interface IDriverRepository : IBaseRepository<Driver>
{
    Task<Driver?> FindByDriverIdAsync(DriverId driverId, CancellationToken cancellationToken);
}
