namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

public record UpdateDriverPhoneCommand(Guid DriverId, string PhoneNumber);
