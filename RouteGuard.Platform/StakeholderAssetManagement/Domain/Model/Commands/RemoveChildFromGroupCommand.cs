namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

public record RemoveChildFromGroupCommand(Guid StudentGroupId, Guid ChildId);
