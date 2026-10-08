using RouteGuard.Platform.FleetRouteManagement.Domain.Model.Queries;
using Route = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;

namespace RouteGuard.Platform.FleetRouteManagement.Application.QueryServices;

/// <summary>
///     Application service that handles the read operations (queries) of the Fleet context.
/// </summary>
public interface IRouteQueryService
{
    /// <summary>Handles retrieving a single route by its identifier.</summary>
    Task<Route?> Handle(GetRouteByIdQuery query, CancellationToken cancellationToken);

    /// <summary>Handles retrieving all routes.</summary>
    Task<IEnumerable<Route>> Handle(GetAllRoutesQuery query, CancellationToken cancellationToken);

    /// <summary>Handles retrieving all routes that belong to an organization.</summary>
    Task<IEnumerable<Route>> Handle(GetRoutesByOrganizationIdQuery query, CancellationToken cancellationToken);
}
