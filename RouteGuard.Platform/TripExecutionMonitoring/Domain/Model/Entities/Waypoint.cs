using RouteGuard.Platform.Shared.Domain.Model.Entities;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using ChildId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.ChildId;

namespace RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Entities;

/// <summary>
///     A stop of the route as it is executed in one trip (table <c>waypoints</c>): it starts
///     <c>PENDING</c> and becomes <c>VISITED</c> when the vehicle crosses its geofence.
/// </summary>
public class Waypoint : IAuditableEntity
{
    public const string Pending = "PENDING";
    public const string Visited = "VISITED";

    protected Waypoint()
    {
        TripId = new TripId(Guid.Empty);
    }

    public Waypoint(TripId tripId, string address, int orderIndex, double latitude, double longitude,
        ChildId? studentId = null)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        StudentId = studentId;
        Address = address;
        OrderIndex = orderIndex;
        Status = Pending;
        Latitude = latitude;
        Longitude = longitude;
    }

    public Guid Id { get; private set; }
    public TripId TripId { get; private set; }
    public ChildId? StudentId { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public int OrderIndex { get; private set; }
    public string Status { get; private set; } = Pending;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTimeOffset? VisitedAt { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsVisited => Status == Visited;

    public void MarkVisited()
    {
        if (IsVisited) return;
        Status = Visited;
        VisitedAt = DateTimeOffset.UtcNow;
    }
}
