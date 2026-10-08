namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

public record RemoveChildFromParentCommand(Guid ParentId, Guid ChildId);
