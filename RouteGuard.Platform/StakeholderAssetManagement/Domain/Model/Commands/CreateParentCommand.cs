namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

public record CreateParentCommand(
    Guid OrganizationId,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);
