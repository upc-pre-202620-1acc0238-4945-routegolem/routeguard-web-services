namespace RouteGuard.Platform.Trip.Interfaces.Rest;

public record LocationUpdateResource(
    Guid? Id,
    double Latitude,
    double Longitude,
    double SpeedKmh,
    int BatteryLevel,
    double Heading,
    long RecordedAt);

public record OfflineBoardingResource(Guid ChildId, string BoardingState, long RecordedAt);

public record OfflineSyncResource(
    IReadOnlyList<LocationUpdateResource>? Locations,
    IReadOnlyList<OfflineBoardingResource>? Boardings);

public record BroadcastResource(string Message);

public record LiveStopResource(Guid Id, string Name, double Latitude, double Longitude, int Order, bool Reached);

public record LiveLocationResource(double Latitude, double Longitude, double SpeedKmh, long RecordedAt);

public record LiveChildResource(Guid ChildId, string Name, string BoardingState);

public record LiveTripResource(
    Guid TripId,
    Guid RouteId,
    string RouteName,
    string DriverName,
    DateTimeOffset? StartedAt,
    int BoardedCount,
    int TotalChildren,
    LiveLocationResource? Location,
    IReadOnlyList<LiveStopResource> Stops,
    IReadOnlyList<LiveChildResource> Children);

/// <summary>
///     Constants and geometry helpers of the live tracking. GPS points (<c>location_records</c>) and
///     the visited stops (<c>waypoints</c>) are stored in the database.
/// </summary>
public static class TripLiveState
{
    /// <summary>Radius around a stop that triggers the "vehicle is close" notification.</summary>
    public const double GeofenceRadiusMeters = 500;

    /// <summary>Haversine distance in meters between two WGS84 points.</summary>
    public static double DistanceMeters(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadius = 6_371_000;
        static double Rad(double deg) => deg * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLng = Rad(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return 2 * earthRadius * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
