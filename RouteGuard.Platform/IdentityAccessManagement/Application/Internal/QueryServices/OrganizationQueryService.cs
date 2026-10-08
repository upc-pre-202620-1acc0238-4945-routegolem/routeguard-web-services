using RouteGuard.Platform.IdentityAccessManagement.Application.QueryServices;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Queries;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Repositories;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.Internal.QueryServices;

public class OrganizationQueryService(IOrganizationRepository organizationRepository) : IOrganizationQueryService
{
    public Task<Organization?> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken) =>
        organizationRepository.FindByOrganizationIdAsync(new OrganizationId(query.OrganizationId), cancellationToken);
}