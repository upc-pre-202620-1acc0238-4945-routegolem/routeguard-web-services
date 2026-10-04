using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;

namespace RouteGuard.Platform.Iam.Domain.Repositories;

public interface IOrganizationRepository : IBaseRepository<Organization>
{
    Task<Organization?> FindByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}
