using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Repositories;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Entities;
using RouteId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.RouteId;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.TripExecutionMonitoring.Infrastructure.Repositories;

/// <summary>
///     EF Core implementation of <see cref="ITripRepository" />.
/// </summary>
/// <remarks>
///     Inherits the generic CRUD operations from <see cref="BaseRepository{TEntity}" /> and adds the
///     Guid-identity finders declared by the domain contract. The aggregate's child collections
///     (<c>Attendances</c> and <c>Incidents</c>) are mapped as owned types, so EF Core loads them
///     automatically together with the aggregate root.
/// </remarks>
/// <param name="context">The application database context.</param>
public class TripRepository(AppDbContext context)
    : BaseRepository<TripAggregate>(context), ITripRepository
{
    /// <inheritdoc />
    public async Task<TripAggregate?> FindByTripIdAsync(TripId tripId, CancellationToken cancellationToken)
    {
        return await Context.Set<TripAggregate>()
            .FirstOrDefaultAsync(trip => trip.Id == tripId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TripAggregate>> FindByRouteIdAsync(RouteId routeId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<TripAggregate>()
            .Where(trip => trip.RouteId == routeId)
            .ToListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task AddSyncBatchAsync(OfflineSyncBatch batch, CancellationToken cancellationToken)
    {
        await Context.Set<OfflineSyncBatch>().AddAsync(batch, cancellationToken);
    }
}