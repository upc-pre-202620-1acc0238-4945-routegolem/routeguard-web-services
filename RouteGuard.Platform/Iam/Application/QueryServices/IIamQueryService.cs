using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.Queries;

namespace RouteGuard.Platform.Iam.Application.QueryServices;

public interface IIamQueryService
{
    Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken);

    Task<Organization?> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken);
}
