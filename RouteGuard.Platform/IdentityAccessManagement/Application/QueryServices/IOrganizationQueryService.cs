using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Queries;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.QueryServices;

public interface IOrganizationQueryService
{
    Task<Organization?> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken);
}