using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Commands;
using RouteGuard.Platform.Shared.Application.Model;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.CommandServices;

public interface IOrganizationCommandService
{
    Task<Result<Organization>> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken);
    Task<Result<Organization>> Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken);
}