using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Resources;

namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Transform;

public static class NotificationResourceFromEntityAssembler
{
    public static NotificationResource ToResourceFromEntity(Notification notification) =>
        new(notification.Id.Identifier, notification.OrganizationId.Identifier, notification.ParentId.Identifier,
            notification.TripId.Identifier, notification.Category.Value, notification.DeliveryState.Value,
            notification.Message.Content, notification.SentAt, 
            notification.Alerts.Select(ToAlertResource).ToList(),
            notification.Announcements.Select(ToAnnouncementResource).ToList());

    private static AlertResource ToAlertResource(Alert alert) =>
        new(alert.Id.Identifier, alert.TriggeredAt, alert.Panic);

    private static AnnouncementResource ToAnnouncementResource(Announcement announcement) =>
        new(announcement.Id.Identifier, announcement.RouteId.Identifier, announcement.Message.Content,
            announcement.PublishedAt);
}
