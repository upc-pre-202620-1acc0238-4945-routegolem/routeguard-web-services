using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Queries;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.QueryServices;

public interface IUserQueryService
{
    Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken);
}