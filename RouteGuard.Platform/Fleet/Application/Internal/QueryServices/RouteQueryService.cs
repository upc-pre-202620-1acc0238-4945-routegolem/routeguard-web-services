using RouteGuard.Platform.Fleet.Application.QueryServices;
using RouteGuard.Platform.Fleet.Domain.Model.Queries;
using RouteGuard.Platform.Fleet.Domain.Model.ValueObjects;
using RouteGuard.Platform.Fleet.Domain.Repositories;
using OrganizationId = RouteGuard.Platform.Fleet.Domain.Model.ValueObjects.OrganizationId;
using Route = RouteGuard.Platform.Fleet.Domain.Model.Aggregates.Route;

namespace RouteGuard.Platform.Fleet.Application.Internal.QueryServices;

/// <summary>
///     Default implementation of <see cref="IRouteQueryService" />.
/// </summary>
/// <param name="routeRepository">The route repository abstraction.</param>


public class RouteQueryService(IRouteRepository routeRepository) : IRouteQueryService
{
    /// <inheritdoc />
    public async Task<Route?> Handle(GetRouteByIdQuery query, CancellationToken cancellationToken)
    {
        return await routeRepository.FindByRouteIdAsync(new RouteId(query.RouteId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Route>> Handle(GetAllRoutesQuery query, CancellationToken cancellationToken)
    {
        return await routeRepository.ListAsync(cancellationToken);
    }
    /// <inheritdoc />
    public async Task<IEnumerable<Route>> Handle(GetRoutesByOrganizationIdQuery query,
        CancellationToken cancellationToken)
    {
        return await routeRepository.FindByOrganizationIdAsync(new OrganizationId(query.OrganizationId),
            cancellationToken);
    }
}