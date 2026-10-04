using RouteGuard.Platform.Iam.Application.QueryServices;
using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.Queries;
using RouteGuard.Platform.Iam.Domain.Model.ValueObjects;
using RouteGuard.Platform.Iam.Domain.Repositories;

namespace RouteGuard.Platform.Iam.Application.Internal.QueryServices;

public class OrganizationQueryService(IOrganizationRepository organizationRepository) : IOrganizationQueryService
{
    public Task<Organization?> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken) =>
        organizationRepository.FindByOrganizationIdAsync(new OrganizationId(query.OrganizationId), cancellationToken);
}