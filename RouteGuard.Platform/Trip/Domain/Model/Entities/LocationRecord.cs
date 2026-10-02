using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Trip.Domain.Model.Entities;

/// <summary>A GPS point sent by the driver's device during a trip (table <c>location_records</c>).</summary>
public class LocationRecord
{
    protected LocationRecord()
    {
        TripId = new TripId(Guid.Empty);
    }

    public LocationRecord(Guid? id, TripId tripId, double latitude, double longitude, double speedKmh,
        int batteryLevel, double heading, DateTimeOffset recordedAt)
    {
        Id = id ?? Guid.NewGuid();
        TripId = tripId;
        Latitude = latitude;
        Longitude = longitude;
        SpeedKmh = speedKmh;
        BatteryLevel = batteryLevel;
        Heading = heading;
        RecordedAt = recordedAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public TripId TripId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double SpeedKmh { get; private set; }
    public int BatteryLevel { get; private set; }
    public double Heading { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
