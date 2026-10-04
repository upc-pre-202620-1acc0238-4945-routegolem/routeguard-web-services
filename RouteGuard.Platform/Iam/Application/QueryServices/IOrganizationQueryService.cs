using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.Queries;

namespace RouteGuard.Platform.Iam.Application.QueryServices;

public interface IOrganizationQueryService
{
    Task<Organization?> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken);
}