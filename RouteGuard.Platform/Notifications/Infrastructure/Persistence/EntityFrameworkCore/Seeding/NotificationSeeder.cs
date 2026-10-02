using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Notifications.Domain.Model.Aggregates;
using RouteGuard.Platform.Notifications.Domain.Model.Commands;
using RouteGuard.Platform.Notifications.Domain.Model.Entities;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using TripAggregate = RouteGuard.Platform.Trip.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.Notifications.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Notifications bounded context with demo notifications and one route announcement.
/// </summary>
public static class NotificationSeeder
{
    private const string WelcomeAnnouncement =
        "Bienvenido a RouteGuard: recuerde mantener actualizados los datos de sus hijos.";

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedTemplatesAsync(context, cancellationToken);

        var seededParentEmail = new Email("parent@routeguard.pe");
        var parent = await context.Set<Parent>().Where(p => p.Email == seededParentEmail)
            .FirstOrDefaultAsync(cancellationToken);
        var trip = await context.Set<TripAggregate>().FirstOrDefaultAsync(cancellationToken);
        if (parent is null || trip is null) return;

        var organizationId = IamSeeder.SeedOrganizationId;
        var notifications = await context.Set<Notification>()
            .Include(notification => notification.Announcements)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            var tripStarted = new Notification(new CreateNotificationCommand(organizationId,
                parent.Id.Identifier, trip.Id.Identifier, "TRIP_STARTED",
                "El bus inicio la Ruta San Martin - Turno Manana."));
            tripStarted.Dispatch();
            tripStarted.MarkDelivered();
            context.Add(tripStarted);
        }

        var announcement = notifications.FirstOrDefault(notification =>
            notification.Category.Value == "ANNOUNCEMENT" &&
            notification.TripId.Identifier == trip.Id.Identifier);

        if (announcement is null)
        {
            announcement = new Notification(new CreateNotificationCommand(organizationId,
                parent.Id.Identifier, trip.Id.Identifier, "ANNOUNCEMENT", WelcomeAnnouncement));
            announcement.Dispatch();
            context.Add(announcement);
        }

        if (announcement.Announcements.All(current => current.RouteId.Identifier != trip.RouteId.Identifier))
            announcement.AddAnnouncement(trip.RouteId.Identifier, WelcomeAnnouncement);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Notification templates (es-PE) with {placeholders}; seeded once, editable in the database.</summary>
    private static async Task SeedTemplatesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Set<NotificationTemplate>().AnyAsync(cancellationToken)) return;

        context.AddRange(
            new NotificationTemplate("GEOFENCE_BREACHED", "es-PE", "El vehiculo se acerca",
                "El vehiculo se acerca a la parada {stop} (a menos de {radius} m)."),
            new NotificationTemplate("SCHOOL_ARRIVAL", "es-PE", "Llegada al colegio",
                "Llegada al colegio: el vehiculo llego a {stop}."),
            new NotificationTemplate("PANIC_ALERT", "es-PE", "Alerta de panico",
                "ALERTA DE PANICO: el conductor activo una alerta en la ruta {route}."),
            new NotificationTemplate("ANNOUNCEMENT", "es-PE", "Aviso del conductor", "{message}"),
            new NotificationTemplate("TRIP_STARTED", "es-PE", "Viaje iniciado", "El bus inicio la ruta {route}."));
        await context.SaveChangesAsync(cancellationToken);
    }
}
