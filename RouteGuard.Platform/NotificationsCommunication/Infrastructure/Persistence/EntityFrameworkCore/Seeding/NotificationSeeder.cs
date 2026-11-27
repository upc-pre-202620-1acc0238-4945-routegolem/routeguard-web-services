using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.FleetRouteManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.NotificationsCommunication.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

/// <summary>
///     Seeds the Notifications bounded context with demo notifications, geofence alerts,
///     route announcements, internal route chat messages, and support tickets.
/// </summary>
public static class NotificationSeeder
{
    private const string WelcomeAnnouncement =
        "Bienvenido a RouteGuard: recuerde mantener actualizados los datos de sus hijos.";

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedTemplatesAsync(context, cancellationToken);

        var trips = await context.Set<TripAggregate>().ToListAsync(cancellationToken);
        var morningTrip = trips
            .Where(t => t.RouteId.Identifier == FleetSeeder.MorningRouteId)
            .OrderByDescending(t => t.IsInProgress())
            .ThenByDescending(t => t.StartTime)
            .FirstOrDefault() ?? trips.FirstOrDefault();

        var afternoonTrip = trips
            .Where(t => t.RouteId.Identifier == FleetSeeder.AfternoonRouteId)
            .OrderByDescending(t => t.IsInProgress())
            .ThenByDescending(t => t.StartTime)
            .FirstOrDefault() ?? morningTrip;

        if (morningTrip is null) return;

        var organizationId = IamSeeder.SeedOrganizationId;
        var existing = await context.Set<Notification>()
            .Include(n => n.Announcements)
            .ToListAsync(cancellationToken);

        EnsureNotification(context, existing,
            Guid.Parse("78000000-0000-0000-0000-000000000001"),
            organizationId,
            StakeholderSeeder.RositaParentId,
            morningTrip.Id.Identifier,
            "TRIP_STARTED",
            "Viaje iniciado",
            "El bus ABC-123 inició la Ruta San Martín - Turno Mañana.",
            delivered: true);

        var geofenceNotifId = Guid.Parse("78000000-0000-0000-0000-000000000002");
        var geofenceNotif = EnsureNotification(context, existing,
            geofenceNotifId,
            organizationId,
            StakeholderSeeder.RositaParentId,
            morningTrip.Id.Identifier,
            "GEOFENCE_BREACHED",
            "El vehículo se acerca",
            "El vehículo se acerca a la parada Parque Kennedy (a menos de 500 m).",
            delivered: false);

        var announcementNotif = EnsureNotification(context, existing,
            Guid.Parse("78000000-0000-0000-0000-000000000003"),
            organizationId,
            StakeholderSeeder.RositaParentId,
            morningTrip.Id.Identifier,
            "ANNOUNCEMENT",
            "Aviso del conductor",
            WelcomeAnnouncement,
            delivered: true);

        if (announcementNotif is not null &&
            announcementNotif.Announcements.All(a => a.RouteId != new NotificationId(morningTrip.RouteId.Identifier)))
        {
            announcementNotif.AddAnnouncement(morningTrip.RouteId.Identifier, WelcomeAnnouncement);
        }

        if (afternoonTrip is not null)
        {
            EnsureNotification(context, existing,
                Guid.Parse("78000000-0000-0000-0000-000000000004"),
                organizationId,
                StakeholderSeeder.JorgeParentId,
                afternoonTrip.Id.Identifier,
                "TRIP_STARTED",
                "Viaje iniciado",
                "El bus DEF-456 inició la Ruta Miraflores - Turno Tarde.",
                delivered: false);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (geofenceNotif is not null)
        {
            var targetNotifId = new NotificationId(geofenceNotifId);
            var hasAlert = await context.Set<GeofenceAlert>()
                .AnyAsync(a => a.NotificationId == targetNotifId, cancellationToken);
            if (!hasAlert)
            {
                context.Add(new GeofenceAlert(
                    new RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects.TripId(morningTrip.Id.Identifier),
                    targetNotifId,
                    GeofenceAlert.Approaching,
                    -12.1155,
                    -77.0308));
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static Notification? EnsureNotification(
        AppDbContext context,
        List<Notification> existing,
        Guid id,
        Guid organizationId,
        Guid parentId,
        Guid tripId,
        string category,
        string title,
        string message,
        bool delivered)
    {
        var nid = new NotificationId(id);
        var found = existing.FirstOrDefault(n => n.Id == nid);
        if (found is not null) return found;

        // Avoid adding duplicate welcome announcement if an identical seeded notification already exists.
        if (category == "ANNOUNCEMENT" &&
            existing.Any(n => n.ParentId.Identifier == parentId &&
                              n.Category.Value == "ANNOUNCEMENT" &&
                              n.Message.Content == message))
        {
            return null;
        }

        var notification = new Notification(new CreateNotificationCommand(
            organizationId, parentId, tripId, category, message));
        context.Entry(notification).Property(n => n.Id).CurrentValue = nid;
        notification.Describe(title, null);
        notification.Dispatch();
        if (delivered) notification.MarkDelivered();
        context.Add(notification);
        existing.Add(notification);
        return notification;
    }

    /// <summary>Notification templates (es-PE & en-US) with {placeholders}; seeded idempotently.</summary>
    public static async Task SeedTemplatesAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<NotificationTemplate>().ToListAsync(cancellationToken);

        var templates = new List<(string EventKey, string Culture, string Title, string Body)>
        {
            ("GEOFENCE_BREACHED", "es-PE", "El vehículo se acerca", "El vehículo se acerca a la parada {stop} (a menos de {radius} m)."),
            ("SCHOOL_ARRIVAL", "es-PE", "Llegada al colegio", "Llegada al colegio: el vehículo llegó a {stop}."),
            ("PANIC_ALERT", "es-PE", "Alerta de pánico", "ALERTA DE PÁNICO: el conductor activó una alerta en la ruta {route}."),
            ("ANNOUNCEMENT", "es-PE", "Aviso del conductor", "{message}"),
            ("TRIP_STARTED", "es-PE", "Viaje iniciado", "El bus inició la ruta {route}."),
            ("GEOFENCE_BREACHED", "en-US", "Vehicle approaching", "The bus is approaching stop {stop} (within {radius} m)."),
            ("SCHOOL_ARRIVAL", "en-US", "Arrived at school", "School arrival: the vehicle reached {stop}."),
            ("PANIC_ALERT", "en-US", "Panic alert", "PANIC ALERT: driver triggered an emergency alert on route {route}."),
            ("ANNOUNCEMENT", "en-US", "Driver announcement", "{message}"),
            ("TRIP_STARTED", "en-US", "Trip started", "The bus started route {route}.")
        };

        foreach (var (eventKey, culture, title, body) in templates)
        {
            if (!existing.Any(t => t.Type == eventKey && t.Locale == culture))
            {
                context.Add(new NotificationTemplate(eventKey, culture, title, body));
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task EnsureCommunicationSeededAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var organizationId = IamSeeder.SeedOrganizationId;

        var existingAnnouncements = await context.Set<RouteAnnouncement>().ToListAsync(cancellationToken);
        var announcements = new[]
        {
            new RouteAnnouncement
            {
                Id = Guid.Parse("74000000-0000-0000-0000-000000000001"),
                OrganizationId = organizationId,
                RouteId = FleetSeeder.MorningRouteId,
                AuthorUserId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                AuthorName = "Carlos Ramirez",
                Title = "Salida puntual - Turno Mañana",
                Message = "Buenos días familias. Recordamos estar 5 minutos antes en su paradero asignado. ¡Buen inicio de jornada!",
                Priority = "IMPORTANT",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new RouteAnnouncement
            {
                Id = Guid.Parse("74000000-0000-0000-0000-000000000002"),
                OrganizationId = organizationId,
                RouteId = FleetSeeder.AfternoonRouteId,
                AuthorUserId = Guid.Parse("b0000000-0000-0000-0000-000000000004"),
                AuthorName = "Luis Torres",
                Title = "Desvío menor por obras en Av. Santa Cruz",
                Message = "Tomaremos calle paralela por 2 cuadras antes de Óvalo Gutiérrez. El horario de entrega se mantiene sin cambios.",
                Priority = "NORMAL",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
            }
        };
        foreach (var a in announcements)
        {
            if (existingAnnouncements.All(x => x.Id != a.Id))
                context.Add(a);
        }

        var existingMessages = await context.Set<ChatMessage>().ToListAsync(cancellationToken);
        var messages = new[]
        {
            new ChatMessage
            {
                Id = Guid.Parse("75000000-0000-0000-0000-000000000001"),
                OrganizationId = organizationId,
                RouteId = FleetSeeder.MorningRouteId,
                SenderUserId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                SenderName = "Carlos Ramirez",
                SenderRole = "DRIVER",
                Message = "Buenos días sra. Rosita, ya salimos de base rumbo a Miraflores y San Miguel.",
                SentAt = DateTimeOffset.UtcNow.AddMinutes(-15)
            },
            new ChatMessage
            {
                Id = Guid.Parse("75000000-0000-0000-0000-000000000002"),
                OrganizationId = organizationId,
                RouteId = FleetSeeder.MorningRouteId,
                SenderUserId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                SenderName = "Rosita Nery",
                SenderRole = "PARENT",
                Message = "Muchas gracias Carlos, Mateo y Luciana ya están listos.",
                SentAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            },
            new ChatMessage
            {
                Id = Guid.Parse("75000000-0000-0000-0000-000000000003"),
                OrganizationId = organizationId,
                RouteId = FleetSeeder.AfternoonRouteId,
                SenderUserId = Guid.Parse("b0000000-0000-0000-0000-000000000004"),
                SenderName = "Luis Torres",
                SenderRole = "DRIVER",
                Message = "Buenas tardes sr. Jorge, iniciando el recorrido hacia Óvalo Gutiérrez.",
                SentAt = DateTimeOffset.UtcNow.AddMinutes(-8)
            }
        };
        foreach (var m in messages)
        {
            if (existingMessages.All(x => x.Id != m.Id))
                context.Add(m);
        }

        var existingTickets = await context.Set<SupportTicket>().ToListAsync(cancellationToken);
        var tickets = new[]
        {
            new SupportTicket
            {
                Id = Guid.Parse("76000000-0000-0000-0000-000000000001"),
                OrganizationId = organizationId,
                UserId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                TicketCode = "#RG-1024",
                Category = "GPS",
                Subject = "Verificación inicial de paradero",
                Message = "Confirmado el radio de alerta de 500m en la puerta principal.",
                Status = "RESOLVED",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new SupportTicket
            {
                Id = Guid.Parse("76000000-0000-0000-0000-000000000002"),
                OrganizationId = organizationId,
                UserId = Guid.Parse("b0000000-0000-0000-0000-000000000005"),
                TicketCode = "#RG-1025",
                Category = "ROUTE",
                Subject = "Actualización de referencia de paradero",
                Message = "Por favor confirmar la esquina exacta en Óvalo Gutiérrez para el recojo de la tarde.",
                Status = "OPEN",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-5)
            }
        };
        foreach (var t in tickets)
        {
            if (existingTickets.All(x => x.Id != t.Id))
                context.Add(t);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
