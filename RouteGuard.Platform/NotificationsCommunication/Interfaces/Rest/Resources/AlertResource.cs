namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Resources;

public record AlertResource(Guid Id, DateTimeOffset TriggeredAt, bool Panic);
