using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Model.Entities;

namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;

public partial class Notification : IAuditableEntity
{
    public NotificationId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public ParentId ParentId { get; private set; }
    public TripId TripId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationDeliveryState DeliveryState { get; private set; }
    public NotificationMessage Message { get; private set; }
    public DateTimeOffset SentAt { get; private set; }

    /// <summary>Push title (report 2.6.2.6.2 <c>notifications.title</c>).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>HIGH for panic alerts, NORMAL otherwise.</summary>
    public string PriorityLevel { get; private set; } = "NORMAL";

    /// <summary>JSON with the identifiers the app needs to open the right screen.</summary>
    public string? DataPayload { get; private set; }

    public DateTimeOffset? DispatchedAt { get; private set; }
    public string? FailureReason { get; private set; }

    /// <summary>FCM token the push was addressed to (empty until push delivery is wired).</summary>
    public string? RecipientToken { get; private set; }

    public string? DevicePlatform { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Alert> Alerts { get; } = new List<Alert>();
    public ICollection<Announcement> Announcements { get; } = new List<Announcement>();

    protected Notification()
    {
        Id = new NotificationId();
        OrganizationId = new OrganizationId(Guid.Empty);
        ParentId = new ParentId(Guid.Empty);
        TripId = new TripId(Guid.Empty);
        Category = new NotificationCategory(string.Empty);
        DeliveryState = new NotificationDeliveryState("Pending");
        Message = new NotificationMessage(string.Empty);
        SentAt = DateTimeOffset.UtcNow;
    }

    public Notification(CreateNotificationCommand command) : this()
    {
        OrganizationId = new OrganizationId(command.OrganizationId);
        ParentId = new ParentId(command.ParentId);
        TripId = new TripId(command.TripId);
        Category = new NotificationCategory(command.Category);
        Message = new NotificationMessage(command.Message);
        DeliveryState = new NotificationDeliveryState("Pending");
        Title = DefaultTitle(command.Category);
        PriorityLevel = command.Category == "PANIC_ALERT" ? "HIGH" : "NORMAL";
    }

    /// <summary>Sets the rendered title and the payload the app uses to open the right screen.</summary>
    public void Describe(string title, string? dataPayload)
    {
        if (!string.IsNullOrWhiteSpace(title)) Title = title;
        DataPayload = dataPayload;
    }

    public void Fail(string reason)
    {
        DeliveryState = new NotificationDeliveryState("Failed");
        FailureReason = reason.Length > 500 ? reason[..500] : reason;
    }

    private static string DefaultTitle(string category) => category switch
    {
        "PANIC_ALERT" => "Alerta de panico",
        "GEOFENCE_BREACHED" => "El vehiculo se acerca",
        "ANNOUNCEMENT" => "Aviso del conductor",
        "TRIP_STARTED" => "Viaje iniciado",
        _ => "RouteGuard"
    };

    public void Dispatch()
    {
        DeliveryState = new NotificationDeliveryState("Dispatched");
        DispatchedAt = DateTimeOffset.UtcNow;
    }

    public void MarkDelivered()
    {
        DeliveryState = new NotificationDeliveryState("Delivered");
    }

    public void AddAlert(bool panic)
    {
        Alerts.Add(new Alert(Id, panic));
    }

    public void AddAnnouncement(Guid routeId, string message)
    {
        Announcements.Add(new Announcement(Id, routeId, message));
    }
}
