using RouteGuard.Platform.FleetRouteManagement.Application.QueryServices;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model.Queries;
using RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.FleetRouteManagement.Domain.Repositories;
using OrganizationId = RouteGuard.Platform.FleetRouteManagement.Domain.Model.ValueObjects.OrganizationId;
using Route = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;

namespace RouteGuard.Platform.FleetRouteManagement.Application.Internal.QueryServices;

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