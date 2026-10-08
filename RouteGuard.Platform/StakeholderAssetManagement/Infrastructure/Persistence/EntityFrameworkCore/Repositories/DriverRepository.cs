using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

namespace RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class DriverRepository(AppDbContext context) : BaseRepository<Driver>(context), IDriverRepository
{
    public Task<Driver?> FindByDriverIdAsync(DriverId driverId, CancellationToken cancellationToken) =>
        Context.Set<Driver>().FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken);
}
