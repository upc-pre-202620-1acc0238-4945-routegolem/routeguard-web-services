using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Trip.Domain.Model.Entities;
using RouteGuard.Platform.Trip.Domain.Model.ValueObjects;
using TripAggregate = RouteGuard.Platform.Trip.Domain.Model.Aggregates.Trip;
using RouteAggregate = RouteGuard.Platform.Fleet.Domain.Model.Aggregates.Route;

namespace RouteGuard.Platform.Trip.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;



/// <summary>
///     EF Core model configuration for the Trip bounded context.
/// </summary>
/// <remarks>
///     <para>
///         Maps the Guid-based identity and reference value objects through value converters and
///         configures the aggregate's child entities (<c>Attendance</c>, <c>Incident</c>) as owned
///         collections, so they share the aggregate's lifecycle and live inside its boundary.
///     </para>
///     <para>
///         Column, table and key names are normalized to snake_case by the global naming convention
///         applied in <c>AppDbContext.OnModelCreating</c>, so only structural mapping is declared here.
///     </para>
/// </remarks>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Applies the Trip context configuration to the EF Core model.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static void ApplyTripConfiguration(this ModelBuilder builder)
    {
        builder.Entity<TripAggregate>(trip =>
        {
            // Identity (Guid value object, application-assigned).
            trip.HasKey(t => t.Id);
            trip.Property(t => t.Id)
                .HasConversion(id => id.Identifier, value => new TripId(value))
                .ValueGeneratedNever()
                .IsRequired();

            // Cross-context references (Shared value objects).
            trip.Property(t => t.OrganizationId)
                .HasConversion(id => id.Identifier, value => new OrganizationId(value))
                .IsRequired();

            trip.Property(t => t.RouteId)
                .HasConversion(id => id.Identifier, value => new RouteId(value))
                .IsRequired();

            trip.Property(t => t.DriverId)
                .HasConversion(id => id.Identifier, value => new DriverId(value))
                .IsRequired();

            // Lifecycle state, persisted as its string value.
            trip.Property(t => t.State)
                .HasConversion(state => state.Value, value => new TripState(value))
                .IsRequired();

            trip.Property(t => t.StartTime);
            trip.Property(t => t.EndTime);
            trip.Property(t => t.CancelledAt);
            
            // Cross-context foreign keys (DB-level integrity only; no navigation, no cascade).
            // Connects Trip → Fleet (route) and Trip → Stakeholder (driver).
            trip.HasOne<RouteAggregate>().WithMany().HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Owned collection: boarding attendances.
            trip.OwnsMany(t => t.Attendances, attendance =>
            {
                attendance.WithOwner().HasForeignKey("TripId");

                attendance.Property(a => a.Id)
                    .HasConversion(id => id.Identifier, value => new AttendanceId(value))
                    .ValueGeneratedNever();
                attendance.HasKey(a => a.Id);

                attendance.Property(a => a.ChildId)
                    .HasConversion(id => id.Identifier, value => new ChildId(value))
                    .IsRequired();

                attendance.Property(a => a.BoardingState)
                    .HasConversion(state => state.Value, value => new BoardingState(value))
                    .IsRequired();

                attendance.Property(a => a.BoardedAt);
            });
            
            // Owned collection: reported incidents.
            trip.OwnsMany(t => t.Incidents, incident =>
            {
                incident.WithOwner().HasForeignKey("TripId");

                incident.Property(i => i.Id)
                    .HasConversion(id => id.Identifier, value => new IncidentId(value))
                    .ValueGeneratedNever();
                incident.HasKey(i => i.Id);

                incident.Property(i => i.Description)
                    .HasConversion(description => description.Value, value => new IncidentDescription(value))
                    .HasMaxLength(IncidentDescription.MaxLength)
                    .IsRequired();

                incident.Property(i => i.ReportedAt).IsRequired();
            });
        });

        // Database design (report 2.6.1.6.2): location_records, waypoints and offline_sync_batches.
        builder.Entity<LocationRecord>(record =>
        {
            record.ToTable("LocationRecords");
            record.HasKey(r => r.Id);
            record.Property(r => r.Id).ValueGeneratedNever();
            record.Property(r => r.TripId).HasConversion(id => id.Identifier, value => new TripId(value)).IsRequired();
            record.Property(r => r.SpeedKmh).HasPrecision(10, 2);
            record.Property(r => r.Heading).HasPrecision(10, 2);
            record.HasIndex(r => new { r.TripId, r.RecordedAt });
            record.HasOne<TripAggregate>().WithMany().HasForeignKey(r => r.TripId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Waypoint>(waypoint =>
        {
            waypoint.ToTable("Waypoints");
            waypoint.HasKey(w => w.Id);
            waypoint.Property(w => w.Id).ValueGeneratedNever();
            waypoint.Property(w => w.TripId).HasConversion(id => id.Identifier, value => new TripId(value)).IsRequired();
            waypoint.Property(w => w.StudentId)
                .HasConversion(id => id == null ? (Guid?)null : id.Identifier,
                    value => value == null ? null : new ChildId(value.Value));
            waypoint.Property(w => w.Address).HasMaxLength(250).IsRequired();
            waypoint.Property(w => w.Status).HasMaxLength(30).IsRequired();
            waypoint.HasIndex(w => new { w.TripId, w.OrderIndex });
            waypoint.HasOne<TripAggregate>().WithMany().HasForeignKey(w => w.TripId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OfflineSyncBatch>(batch =>
        {
            batch.ToTable("OfflineSyncBatches");
            batch.HasKey(b => b.Id);
            batch.Property(b => b.Id).ValueGeneratedNever();
            batch.Property(b => b.TripId).HasConversion(id => id.Identifier, value => new TripId(value)).IsRequired();
            batch.Property(b => b.RawPayload).HasColumnType("json").IsRequired();
            batch.HasOne<TripAggregate>().WithMany().HasForeignKey(b => b.TripId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}