using RouteGuard.Platform.Notifications.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Model.Entities;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Notifications.Domain.Model.Entities;

/// <summary>Alert generated when the vehicle crosses the geofence of a stop (table <c>geofence_alerts</c>).</summary>
public class GeofenceAlert : IAuditableEntity
{
    public const string Approaching = "APPROACHING";
    public const string SchoolArrival = "SCHOOL_ARRIVAL";

    protected GeofenceAlert()
    {
        TripId = new TripId(Guid.Empty);
        NotificationId = new NotificationId(Guid.Empty);
    }

    public GeofenceAlert(TripId tripId, NotificationId notificationId, string alertType, double latitude,
        double longitude, ChildId? studentId = null)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        NotificationId = notificationId;
        AlertType = alertType;
        Latitude = latitude;
        Longitude = longitude;
        StudentId = studentId;
        TriggeredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public TripId TripId { get; private set; }
    public ChildId? StudentId { get; private set; }
    public NotificationId NotificationId { get; private set; }
    public string AlertType { get; private set; } = Approaching;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTimeOffset TriggeredAt { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
