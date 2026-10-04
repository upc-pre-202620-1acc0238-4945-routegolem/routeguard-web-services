using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class DriverRepository(AppDbContext context) : BaseRepository<Driver>(context), IDriverRepository
{
    public Task<Driver?> FindByDriverIdAsync(DriverId driverId, CancellationToken cancellationToken) =>
        Context.Set<Driver>().FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken);
}
