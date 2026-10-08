using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.CommandServices;

public interface IParentCommandService
{
    Task<Result<Parent>> Handle(CreateParentCommand command, CancellationToken cancellationToken);
    Task<Result<Parent>> Handle(UpdateParentCommand command, CancellationToken cancellationToken);
    Task<Result<Parent>> Handle(DeleteParentCommand command, CancellationToken cancellationToken);
    Task<Result<Parent>> Handle(AddChildToParentCommand command, CancellationToken cancellationToken);
    Task<Result<Parent>> Handle(RemoveChildFromParentCommand command, CancellationToken cancellationToken);
}