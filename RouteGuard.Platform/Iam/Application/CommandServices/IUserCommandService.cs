using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.Commands;
using RouteGuard.Platform.Shared.Application.Model;

namespace RouteGuard.Platform.Iam.Application.CommandServices;

public interface IUserCommandService
{
    Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken);
    Task<Result<(User User, string Token)>> Handle(SignInCommand command, CancellationToken cancellationToken);
}