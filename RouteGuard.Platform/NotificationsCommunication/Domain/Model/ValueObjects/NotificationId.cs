namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects;

public record NotificationId(Guid Identifier)
{
    public NotificationId() : this(Guid.NewGuid())
    {
    }
}
