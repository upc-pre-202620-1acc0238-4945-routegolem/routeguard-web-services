namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;

public record AddChildToGroupCommand(Guid StudentGroupId, Guid ChildId);
