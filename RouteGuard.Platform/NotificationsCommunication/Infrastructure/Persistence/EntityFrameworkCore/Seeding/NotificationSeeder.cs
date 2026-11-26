using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using TripAggregate = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates.Trip;

namespace RouteGuard.Platform.NotificationsCommunication.Infrastructure.Persistence.EntityFrameworkCore.Seeding;

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
                "El bus inició la Ruta San Martín - Turno Mañana."));
            tripStarted.Dispatch();
            tripStarted.MarkDelivered();
            context.Add(tripStarted);
        }

        var announcement = notifications.FirstOrDefault(notification =>
            notification.Category.Value == "ANNOUNCEMENT" &&
            notification.TripId == new RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects.TripId(trip.Id.Identifier));

        if (announcement is null)
        {
            announcement = new Notification(new CreateNotificationCommand(organizationId,
                parent.Id.Identifier, trip.Id.Identifier, "ANNOUNCEMENT", WelcomeAnnouncement));
            announcement.Dispatch();
            context.Add(announcement);
        }

        if (announcement.Announcements.All(current => current.RouteId != new RouteGuard.Platform.NotificationsCommunication.Domain.Model.ValueObjects.NotificationId(trip.RouteId.Identifier)))
            announcement.AddAnnouncement(trip.RouteId.Identifier, WelcomeAnnouncement);

        await context.SaveChangesAsync(cancellationToken);
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
        var routeId = Guid.Parse("f0000000-0000-0000-0000-000000000001");

        if (!await context.Set<RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.RouteAnnouncement>().AnyAsync(cancellationToken))
        {
            context.Add(new RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.RouteAnnouncement
            {
                Id = Guid.Parse("74000000-0000-0000-0000-000000000001"),
                OrganizationId = organizationId,
                RouteId = routeId,
                AuthorUserId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                AuthorName = "Carlos Ramirez",
                Title = "Salida puntual - Turno Mañana",
                Message = "Buenos días familias. Recordamos estar 5 minutos antes en su paradero asignado. ¡Buen inicio de jornada!",
                Priority = "IMPORTANT",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await context.Set<RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.ChatMessage>().AnyAsync(cancellationToken))
        {
            context.AddRange(
                new RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.ChatMessage
                {
                    Id = Guid.Parse("75000000-0000-0000-0000-000000000001"),
                    OrganizationId = organizationId,
                    RouteId = routeId,
                    SenderUserId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                    SenderName = "Carlos Ramirez",
                    SenderRole = "DRIVER",
                    Message = "Buenos días sra. Rosita, ya salimos de base rumbo a San Borja.",
                    SentAt = DateTimeOffset.UtcNow.AddMinutes(-15)
                },
                new RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.ChatMessage
                {
                    Id = Guid.Parse("75000000-0000-0000-0000-000000000002"),
                    OrganizationId = organizationId,
                    RouteId = routeId,
                    SenderUserId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                    SenderName = "Rosita Nery",
                    SenderRole = "PARENT",
                    Message = "Muchas gracias Carlos, Mateo y Luciana ya están listos.",
                    SentAt = DateTimeOffset.UtcNow.AddMinutes(-10)
                });
        }

        if (!await context.Set<RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.SupportTicket>().AnyAsync(cancellationToken))
        {
            context.Add(new RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities.SupportTicket
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
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
