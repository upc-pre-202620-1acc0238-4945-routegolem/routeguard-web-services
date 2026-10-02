namespace RouteGuard.Platform.Notifications.Interfaces.Rest.Resources;

public record PublishAnnouncementResource(Guid RouteId, string Message);
