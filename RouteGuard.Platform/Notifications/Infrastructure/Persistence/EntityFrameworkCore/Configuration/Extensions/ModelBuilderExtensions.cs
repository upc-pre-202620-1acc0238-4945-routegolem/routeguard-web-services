using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Notifications.Domain.Model.Aggregates;
using RouteGuard.Platform.Notifications.Domain.Model.Entities;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Notifications.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Notifications.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyNotificationConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Notification>(notification =>
        {
            notification.ToTable("Notifications");
            notification.HasKey(n => n.Id);
            notification.Property(n => n.Id)
                .HasConversion(id => id.Identifier, value => new NotificationId(value))
                .ValueGeneratedNever();
            notification.Property(n => n.OrganizationId)
                .HasConversion(id => id.Identifier, value => new NotificationId(value));
            notification.Property(n => n.ParentId)
                .HasConversion(id => id.Identifier, value => new NotificationId(value));
            notification.Property(n => n.TripId)
                .HasConversion(id => id.Identifier, value => new NotificationId(value));
            notification.Property(n => n.Category)
                .HasConversion(category => category.Value, value => new NotificationCategory(value))
                .HasMaxLength(50).IsRequired();
            notification.Property(n => n.DeliveryState)
                .HasConversion(state => state.Value, value => new NotificationDeliveryState(value))
                .HasMaxLength(20).IsRequired();
            notification.Property(n => n.Message)
                .HasConversion(message => message.Content, value => new NotificationMessage(value))
                .HasMaxLength(1000).IsRequired();
            notification.Property(n => n.SentAt).IsRequired();
            notification.Property(n => n.Title).HasMaxLength(255).IsRequired();
            notification.Property(n => n.PriorityLevel).HasMaxLength(30).IsRequired();
            notification.Property(n => n.DataPayload).HasColumnType("json");
            notification.Property(n => n.FailureReason).HasMaxLength(500);
            notification.Property(n => n.RecipientToken).HasMaxLength(512);
            notification.Property(n => n.DevicePlatform).HasMaxLength(20);

            // Alerts Collection
            notification.OwnsMany(n => n.Alerts, alert =>
            {
                alert.WithOwner().HasForeignKey("OwnerNotificationId");
                alert.HasKey(a => a.Id);
                alert.Property(a => a.Id)
                    .HasConversion(id => id.Identifier, value => new AlertId(value))
                    .ValueGeneratedNever();
                alert.Property(a => a.NotificationId)
                    .HasConversion(id => id.Identifier, value => new NotificationId(value));
                alert.Property(a => a.TriggeredAt).IsRequired();
                alert.Property(a => a.Panic).IsRequired();
            });

            // Announcements Collection
            notification.OwnsMany(n => n.Announcements, announcement =>
            {
                announcement.WithOwner().HasForeignKey("OwnerNotificationId");
                announcement.HasKey(a => a.Id);
                announcement.Property(a => a.Id)
                    .HasConversion(id => id.Identifier, value => new AnnouncementId(value))
                    .ValueGeneratedNever();
                announcement.Property(a => a.NotificationId)
                    .HasConversion(id => id.Identifier, value => new NotificationId(value));
                announcement.Property(a => a.RouteId)
                    .HasConversion(id => id.Identifier, value => new NotificationId(value));
                announcement.Property(a => a.Message)
                    .HasConversion(message => message.Content, value => new NotificationMessage(value))
                    .HasMaxLength(1000).IsRequired();
                announcement.Property(a => a.PublishedAt).IsRequired();
            });
        });

        // Database design (report 2.6.2.6.2): geofence_alerts, notification_templates, device_tokens.
        builder.Entity<GeofenceAlert>(alert =>
        {
            alert.ToTable("GeofenceAlerts");
            alert.HasKey(a => a.Id);
            alert.Property(a => a.Id).ValueGeneratedNever();
            alert.Property(a => a.TripId).HasConversion(id => id.Identifier, value => new TripId(value)).IsRequired();
            alert.Property(a => a.StudentId)
                .HasConversion(id => id == null ? (Guid?)null : id.Identifier,
                    value => value == null ? null : new ChildId(value.Value));
            alert.Property(a => a.NotificationId)
                .HasConversion(id => id.Identifier, value => new NotificationId(value)).IsRequired();
            alert.Property(a => a.AlertType).HasMaxLength(30).IsRequired();
            alert.Property(a => a.Latitude).HasPrecision(10, 7);
            alert.Property(a => a.Longitude).HasPrecision(10, 7);
            alert.HasOne<Notification>().WithMany().HasForeignKey(a => a.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NotificationTemplate>(template =>
        {
            template.ToTable("NotificationTemplates");
            template.HasKey(t => t.Id);
            template.Property(t => t.Id).ValueGeneratedNever();
            template.Property(t => t.Type).HasMaxLength(80).IsRequired();
            template.Property(t => t.Locale).HasMaxLength(20).IsRequired();
            template.Property(t => t.TitleTemplate).HasMaxLength(255).IsRequired();
            template.Property(t => t.BodyTemplate).HasColumnType("text").IsRequired();
            template.HasIndex(t => new { t.Type, t.Locale }).IsUnique();
        });

        builder.Entity<DeviceToken>(deviceToken =>
        {
            deviceToken.ToTable("DeviceTokens");
            deviceToken.HasKey(d => d.Id);
            deviceToken.Property(d => d.Id).ValueGeneratedNever();
            deviceToken.Property(d => d.UserId)
                .HasConversion(id => id.Identifier, value => new UserId(value)).IsRequired();
            deviceToken.Property(d => d.Token).HasMaxLength(512).IsRequired();
            deviceToken.Property(d => d.Platform).HasMaxLength(20).IsRequired();
            deviceToken.HasIndex(d => d.Token).IsUnique();
            deviceToken.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}