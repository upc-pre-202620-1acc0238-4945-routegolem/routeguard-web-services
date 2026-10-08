namespace RouteGuard.Platform.FleetRouteManagement.Interfaces.Rest.Resources;

public record CreateVehicleResource(
    Guid OrganizationId,
    string Plate,
    string Model,
    int Capacity,
    string Status);
