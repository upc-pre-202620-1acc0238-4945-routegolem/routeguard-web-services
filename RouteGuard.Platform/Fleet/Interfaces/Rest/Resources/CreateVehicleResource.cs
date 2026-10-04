namespace RouteGuard.Platform.Fleet.Interfaces.Rest.Resources;

public record CreateVehicleResource(
    Guid OrganizationId,
    string Plate,
    string Model,
    int Capacity,
    string Status);
