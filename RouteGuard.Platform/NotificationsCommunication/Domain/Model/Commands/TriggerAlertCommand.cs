namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;

public record TriggerAlertCommand(Guid NotificationId, bool Panic);
