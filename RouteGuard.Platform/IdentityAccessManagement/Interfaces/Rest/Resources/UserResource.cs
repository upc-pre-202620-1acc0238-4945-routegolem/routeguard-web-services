namespace RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest.Resources;

public record UserResource(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string RoleTier,
    Guid? OrganizationId);
