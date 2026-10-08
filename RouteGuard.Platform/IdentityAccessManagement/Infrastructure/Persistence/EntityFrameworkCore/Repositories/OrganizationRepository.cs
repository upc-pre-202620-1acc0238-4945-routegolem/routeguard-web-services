using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Repositories;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;


namespace RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class OrganizationRepository(AppDbContext context) : BaseRepository<Organization>(context),
    IOrganizationRepository
{
    public Task<Organization?> FindByOrganizationIdAsync(OrganizationId organizationId,
        CancellationToken cancellationToken) =>
        Context.Set<Organization>().FirstOrDefaultAsync(organization => organization.Id == organizationId,
            cancellationToken);
}
