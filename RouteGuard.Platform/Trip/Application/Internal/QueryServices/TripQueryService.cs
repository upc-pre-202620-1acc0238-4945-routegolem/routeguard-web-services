using RouteGuard.Platform.Trip.Application.QueryServices;
using RouteGuard.Platform.Trip.Domain.Model.Queries;
using RouteGuard.Platform.Trip.Domain.Model.ValueObjects;
using RouteGuard.Platform.Trip.Domain.Repositories;
using RouteId = RouteGuard.Platform.Trip.Domain.Model.ValueObjects.RouteId;
using TripAggregate = RouteGuard.Platform.Trip.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.Trip.Application.Internal.QueryServices;

/// <summary>
///     Default implementation of <see cref="ITripQueryService" />.
/// </summary>
/// <remarks>Depends on the repository abstraction only, never on its implementation.</remarks>
/// <param name="tripRepository">The trip repository.</param>
public class TripQueryService(ITripRepository tripRepository) : ITripQueryService
{
    /// <inheritdoc />
    public async Task<TripAggregate?> Handle(GetTripByIdQuery query, CancellationToken cancellationToken)
    {
        return await tripRepository.FindByTripIdAsync(new TripId(query.TripId), cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<TripAggregate>> Handle(GetAllTripsQuery query, CancellationToken cancellationToken)
    {
        return await tripRepository.ListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<TripAggregate>> Handle(GetTripsByRouteIdQuery query,
        CancellationToken cancellationToken)
    {
        return await tripRepository.FindByRouteIdAsync(new RouteId(query.RouteId), cancellationToken);
    }
}