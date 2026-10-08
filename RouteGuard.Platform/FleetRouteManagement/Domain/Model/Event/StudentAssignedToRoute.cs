using RouteGuard.Platform.Shared.Domain.Model.Events;

namespace RouteGuard.Platform.FleetRouteManagement.Domain.Model.Event;

/// <summary>Domain event raised when a child (student) is assigned to a route.</summary>
/// <param name="RouteId">The route the child was assigned to.</param>
/// <param name="ChildId">The assigned child.</param>
public record StudentAssignedToRouteEvent(Guid RouteId, Guid ChildId) : IEvent;