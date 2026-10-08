using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest.Resources;

namespace RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest.Transform;

public static class IamResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User user) =>
        new(user.Id.Identifier, user.FullName.FirstName, user.FullName.LastName, user.Email.Value, user.Role.Value,
            user.OrganizationId?.Identifier);

    public static AuthenticatedUserResource ToAuthenticatedResourceFromEntity(User user, string token) =>
        new(user.Id.Identifier, user.FullName.FirstName, user.FullName.LastName, user.Email.Value, user.Role.Value,
            user.OrganizationId?.Identifier, token);

    public static OrganizationResource ToResourceFromEntity(Organization organization) =>
        new(organization.Id.Identifier, organization.Name.Value, organization.Status.Value, organization.CreatedAt);
}
