namespace RouteGuard.Platform.StakeholderAssetManagement.Interfaces.Rest.Resources;

public record ParentResource(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    IEnumerable<ChildResource> Children);
