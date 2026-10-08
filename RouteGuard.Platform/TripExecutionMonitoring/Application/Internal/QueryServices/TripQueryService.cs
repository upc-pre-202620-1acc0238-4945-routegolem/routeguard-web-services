using RouteGuard.Platform.TripExecutionMonitoring.Application.QueryServices;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Queries;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Repositories;
using RouteId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.RouteId;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.TripExecutionMonitoring.Application.Internal.QueryServices;

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