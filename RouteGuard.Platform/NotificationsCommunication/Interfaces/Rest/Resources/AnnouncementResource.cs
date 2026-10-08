namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Resources;

public record AnnouncementResource(Guid Id, Guid RouteId, string Message, DateTimeOffset PublishedAt);
