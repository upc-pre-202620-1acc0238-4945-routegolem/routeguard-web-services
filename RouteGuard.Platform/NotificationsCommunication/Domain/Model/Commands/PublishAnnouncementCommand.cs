namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;

public record PublishAnnouncementCommand(Guid NotificationId, Guid RouteId, string Message);
