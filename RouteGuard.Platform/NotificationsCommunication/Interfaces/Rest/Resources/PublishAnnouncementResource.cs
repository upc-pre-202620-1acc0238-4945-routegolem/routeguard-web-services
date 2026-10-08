namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Resources;

public record PublishAnnouncementResource(Guid RouteId, string Message);
