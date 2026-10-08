namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;

public record CreateNotificationCommand(Guid OrganizationId, Guid ParentId, Guid TripId, string Category, string Message);