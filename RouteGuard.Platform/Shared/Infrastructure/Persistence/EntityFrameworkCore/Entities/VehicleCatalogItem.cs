namespace RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;

/// <summary>
///     Persistence model for the standalone vehicle catalog consumed by the web app compatibility
///     endpoints.
/// </summary>
public class VehicleCatalogItem
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Plate { get; set; } = string.Empty;

    public string Brand { get; set; } = "Mercedes-Benz";

    public string Model { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public string SoatPolicy { get; set; } = "SOAT-2026-88412";

    public string SoatInsurer { get; set; } = "Rímac Seguros";

    public string SoatExpiresAt { get; set; } = "2026-12-15";

    public bool SoatValid { get; set; } = true;

    public void Update(string plate, string model, int capacity, string status,
        string? brand = null, string? soatPolicy = null, string? soatInsurer = null,
        string? soatExpiresAt = null, bool? soatValid = null)
    {
        Plate = plate;
        Model = model;
        Capacity = capacity;
        Status = status;
        if (!string.IsNullOrWhiteSpace(brand)) Brand = brand.Trim();
        if (!string.IsNullOrWhiteSpace(soatPolicy)) SoatPolicy = soatPolicy.Trim();
        if (!string.IsNullOrWhiteSpace(soatInsurer)) SoatInsurer = soatInsurer.Trim();
        if (!string.IsNullOrWhiteSpace(soatExpiresAt)) SoatExpiresAt = soatExpiresAt.Trim();
        if (soatValid.HasValue) SoatValid = soatValid.Value;
    }
}
