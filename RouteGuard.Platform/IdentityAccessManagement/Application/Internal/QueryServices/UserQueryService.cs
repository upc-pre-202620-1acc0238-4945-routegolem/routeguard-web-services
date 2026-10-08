using RouteGuard.Platform.IdentityAccessManagement.Application.QueryServices;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Queries;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Repositories;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.Internal.QueryServices;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    public Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken) =>
        userRepository.FindByUserIdAsync(new UserId(query.UserId), cancellationToken);

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken) =>
        await userRepository.ListAsync(cancellationToken);
}