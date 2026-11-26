namespace RouteGuard.Platform.FleetRouteManagement.Interfaces.Rest.Resources;

/// <summary>Output resource representing the vehicle assigned to a route.</summary>
/// <param name="Id">The vehicle identifier (Guid as string).</param>
/// <param name="Plate">The license plate.</param>
/// <param name="Model">The vehicle model.</param>
/// <param name="Brand">The vehicle brand.</param>
/// <param name="Capacity">The seating capacity.</param>
public record VehicleResource(
    Guid Id,
    Guid OrganizationId,
    string Plate,
    string Brand,
    string Model,
    int Capacity,
    string Status,
    string SoatPolicy = "SOAT-2026-88412",
    string SoatInsurer = "Rímac Seguros",
    string SoatExpiresAt = "2026-12-15",
    bool SoatValid = true
);