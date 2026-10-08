namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

public record UpdateParentCommand(
    Guid ParentId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);
