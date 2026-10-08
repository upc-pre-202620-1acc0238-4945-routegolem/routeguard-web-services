using RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;

public class Alert
{
    public AlertId Id { get; private set; }
    public NotificationId NotificationId { get; private set; }
    public DateTimeOffset TriggeredAt { get; private set; }
    public bool Panic { get; private set; }

    protected Alert()
    {
        Id = new AlertId();
        NotificationId = new NotificationId(Guid.Empty);
    }

    public Alert(NotificationId notificationId, bool panic)
    {
        Id = new AlertId();
        NotificationId = notificationId;
        TriggeredAt = DateTimeOffset.UtcNow;
        Panic = panic;
    }
}
