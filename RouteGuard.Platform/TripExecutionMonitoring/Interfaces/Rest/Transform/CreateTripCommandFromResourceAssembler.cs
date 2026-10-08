using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Commands;
using RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest.Resources;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembler that converts a <see cref="CreateTripResource" /> into a <see cref="CreateTripCommand" />.
/// </summary>
public static class CreateTripCommandFromResourceAssembler
{
    /// <summary>Converts the input resource into a domain command, parsing the Guid identifiers.</summary>
    /// <param name="resource">The create-trip resource. Must not be null.</param>
    /// <returns>The resulting <see cref="CreateTripCommand" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="resource" /> is null.</exception>
    /// <exception cref="FormatException">Thrown when an identifier is not a valid Guid.</exception>
    public static CreateTripCommand ToCommandFromResource(CreateTripResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateTripCommand(
            Guid.Parse(resource.OrganizationId),
            Guid.Parse(resource.RouteId),
            Guid.Parse(resource.DriverId));
    }
}