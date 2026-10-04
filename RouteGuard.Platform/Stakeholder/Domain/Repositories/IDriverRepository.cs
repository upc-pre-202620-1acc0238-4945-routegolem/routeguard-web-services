using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Stakeholder.Domain.Repositories;

public interface IDriverRepository : IBaseRepository<Driver>
{
    Task<Driver?> FindByDriverIdAsync(DriverId driverId, CancellationToken cancellationToken);
}
