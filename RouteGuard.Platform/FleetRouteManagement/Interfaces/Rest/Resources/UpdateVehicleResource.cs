namespace RouteGuard.Platform.FleetRouteManagement.Interfaces.Rest.Resources;

public record UpdateVehicleResource(
    string Plate,
    string Model,
    int Capacity,
    string Status);
