namespace RouteGuard.Platform.Stakeholder.Domain.Model.Commands;

public record UpdateDriverPhoneCommand(Guid DriverId, string PhoneNumber);
