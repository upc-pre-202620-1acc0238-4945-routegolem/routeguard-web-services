using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.ValueObjects;
using RouteGuard.Platform.Iam.Domain.Repositories;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;


namespace RouteGuard.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class OrganizationRepository(AppDbContext context) : BaseRepository<Organization>(context),
    IOrganizationRepository
{
    public Task<Organization?> FindByOrganizationIdAsync(OrganizationId organizationId,
        CancellationToken cancellationToken) =>
        Context.Set<Organization>().FirstOrDefaultAsync(organization => organization.Id == organizationId,
            cancellationToken);
}
