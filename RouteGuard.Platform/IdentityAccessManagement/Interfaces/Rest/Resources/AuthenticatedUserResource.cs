namespace RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest.Resources;

public record AuthenticatedUserResource(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string RoleTier,
    Guid? OrganizationId,
    string Token);
