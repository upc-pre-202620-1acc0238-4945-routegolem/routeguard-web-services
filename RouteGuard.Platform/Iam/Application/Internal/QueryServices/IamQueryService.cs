using RouteGuard.Platform.Iam.Application.QueryServices;
using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.Queries;
using RouteGuard.Platform.Iam.Domain.Repositories;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Iam.Application.Internal.QueryServices;

public class IamQueryService(IUserRepository userRepository, IOrganizationRepository organizationRepository)
    : IIamQueryService
{
    public Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken) =>
        userRepository.FindByUserIdAsync(new UserId(query.UserId), cancellationToken);

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken) =>
        await userRepository.ListAsync(cancellationToken);

    public Task<Organization?> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken) =>
        organizationRepository.FindByOrganizationIdAsync(new OrganizationId(query.OrganizationId),
            cancellationToken);
}
