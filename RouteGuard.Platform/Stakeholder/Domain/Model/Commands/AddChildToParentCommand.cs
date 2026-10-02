namespace RouteGuard.Platform.Stakeholder.Domain.Model.Commands;

public record AddChildToParentCommand(Guid ParentId, string FirstName, string LastName, int Age);
