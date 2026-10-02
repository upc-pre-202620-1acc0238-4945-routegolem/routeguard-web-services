namespace RouteGuard.Platform.Stakeholder.Domain.Model.Commands;

public record RemoveChildFromParentCommand(Guid ParentId, Guid ChildId);
