namespace RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Commands;

public record UpdateOrganizationCommand(Guid OrganizationId, string Name);
