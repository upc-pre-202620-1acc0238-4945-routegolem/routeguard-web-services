namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Resources;

public record CreateNotificationResource(Guid OrganizationId, Guid ParentId, Guid TripId, string Category, string Message);
