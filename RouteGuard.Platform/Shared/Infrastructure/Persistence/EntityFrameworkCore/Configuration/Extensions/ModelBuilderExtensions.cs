using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;

namespace RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;


/// <summary>
///     Model builder extensions for the database context
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplySharedConfiguration(this ModelBuilder builder)
    {
        builder.Entity<VehicleCatalogItem>(vehicle =>
        {
            vehicle.ToTable("Vehicle");
            vehicle.HasKey(v => v.Id);
            vehicle.Property(v => v.Id).ValueGeneratedNever();
            vehicle.Property(v => v.OrganizationId).IsRequired();
            vehicle.Property(v => v.Plate).IsRequired().HasMaxLength(20);
            vehicle.Property(v => v.Brand).IsRequired().HasMaxLength(60).HasDefaultValue("Mercedes-Benz");
            vehicle.Property(v => v.Model).IsRequired().HasMaxLength(80);
            vehicle.Property(v => v.Capacity).IsRequired();
            vehicle.Property(v => v.Status).IsRequired().HasMaxLength(20);
            vehicle.Property(v => v.SoatPolicy).IsRequired().HasMaxLength(60).HasDefaultValue("SOAT-2026-88412");
            vehicle.Property(v => v.SoatInsurer).IsRequired().HasMaxLength(80).HasDefaultValue("Rímac Seguros");
            vehicle.Property(v => v.SoatExpiresAt).IsRequired().HasMaxLength(20).HasDefaultValue("2026-12-15");
            vehicle.Property(v => v.SoatValid).IsRequired().HasDefaultValue(true);
        });

        builder.Entity<PasswordResetToken>(token =>
        {
            token.ToTable("PasswordResetTokens");
            token.HasKey(t => t.Id);
            token.Property(t => t.Id).ValueGeneratedNever();
            token.Property(t => t.Email).IsRequired().HasMaxLength(150);
            token.Property(t => t.Code).IsRequired().HasMaxLength(16);
            token.Property(t => t.ExpiresAt).IsRequired();
            token.Property(t => t.Used).IsRequired();
            token.Property(t => t.CreatedAt).IsRequired();
        });

        builder.Entity<AuthorizedCaregiver>(caregiver =>
        {
            caregiver.ToTable("AuthorizedCaregivers");
            caregiver.HasKey(c => c.Id);
            caregiver.Property(c => c.Id).ValueGeneratedNever();
            caregiver.Property(c => c.OrganizationId).IsRequired();
            caregiver.Property(c => c.ParentId).IsRequired();
            caregiver.Property(c => c.FullName).IsRequired().HasMaxLength(120);
            caregiver.Property(c => c.Relationship).IsRequired().HasMaxLength(80);
            caregiver.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(30);
            caregiver.Property(c => c.DocumentNumber).HasMaxLength(30);
            caregiver.Property(c => c.PhotoData).HasColumnType("text");
            caregiver.Property(c => c.CreatedAt).IsRequired();
        });

        builder.Entity<SupportTicket>(ticket =>
        {
            ticket.ToTable("SupportTickets");
            ticket.HasKey(t => t.Id);
            ticket.Property(t => t.Id).ValueGeneratedNever();
            ticket.Property(t => t.OrganizationId).IsRequired();
            ticket.Property(t => t.UserId).IsRequired();
            ticket.Property(t => t.TicketCode).IsRequired().HasMaxLength(30);
            ticket.Property(t => t.Category).IsRequired().HasMaxLength(60);
            ticket.Property(t => t.Subject).IsRequired().HasMaxLength(160);
            ticket.Property(t => t.Message).IsRequired().HasMaxLength(2000);
            ticket.Property(t => t.Status).IsRequired().HasMaxLength(30);
            ticket.Property(t => t.CreatedAt).IsRequired();
        });

        builder.Entity<TripRating>(rating =>
        {
            rating.ToTable("TripRatings");
            rating.HasKey(r => r.Id);
            rating.Property(r => r.Id).ValueGeneratedNever();
            rating.Property(r => r.OrganizationId).IsRequired();
            rating.Property(r => r.TripId).IsRequired();
            rating.Property(r => r.UserId).IsRequired();
            rating.Property(r => r.Stars).IsRequired();
            rating.Property(r => r.Comment).HasMaxLength(600);
            rating.Property(r => r.CreatedAt).IsRequired();
        });

        builder.Entity<AbsenceReport>(absence =>
        {
            absence.ToTable("AbsenceReports");
            absence.HasKey(a => a.Id);
            absence.Property(a => a.Id).ValueGeneratedNever();
            absence.Property(a => a.OrganizationId).IsRequired();
            absence.Property(a => a.ParentId).IsRequired();
            absence.Property(a => a.ChildId).IsRequired();
            absence.Property(a => a.ChildName).IsRequired().HasMaxLength(120);
            absence.Property(a => a.RouteId).IsRequired();
            absence.Property(a => a.AbsenceDate).IsRequired().HasMaxLength(20);
            absence.Property(a => a.Reason).IsRequired().HasMaxLength(120);
            absence.Property(a => a.Notes).HasMaxLength(500);
            absence.Property(a => a.CreatedAt).IsRequired();
        });

        builder.Entity<VehicleMaintenance>(m =>
        {
            m.ToTable("VehicleMaintenances");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).ValueGeneratedNever();
            m.Property(x => x.OrganizationId).IsRequired();
            m.Property(x => x.VehicleId).IsRequired();
            m.Property(x => x.VehiclePlate).IsRequired().HasMaxLength(20);
            m.Property(x => x.MaintenanceType).IsRequired().HasMaxLength(80);
            m.Property(x => x.Description).IsRequired().HasMaxLength(500);
            m.Property(x => x.Workshop).IsRequired().HasMaxLength(120);
            m.Property(x => x.Cost).HasPrecision(10, 2).IsRequired();
            m.Property(x => x.OdometerKm).IsRequired();
            m.Property(x => x.PerformedAt).IsRequired().HasMaxLength(20);
            m.Property(x => x.NextDueDate).IsRequired().HasMaxLength(20);
            m.Property(x => x.Status).IsRequired().HasMaxLength(30);
            m.Property(x => x.CreatedAt).IsRequired();
        });

        builder.Entity<OperationalExpense>(e =>
        {
            e.ToTable("OperationalExpenses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.OrganizationId).IsRequired();
            e.Property(x => x.VehicleId).IsRequired();
            e.Property(x => x.VehiclePlate).IsRequired().HasMaxLength(20);
            e.Property(x => x.DriverName).IsRequired().HasMaxLength(120);
            e.Property(x => x.Category).IsRequired().HasMaxLength(40);
            e.Property(x => x.Description).IsRequired().HasMaxLength(400);
            e.Property(x => x.Amount).HasPrecision(10, 2).IsRequired();
            e.Property(x => x.ExpenseDate).IsRequired().HasMaxLength(20);
            e.Property(x => x.CreatedAt).IsRequired();
        });

        builder.Entity<RouteAnnouncement>(a =>
        {
            a.ToTable("RouteAnnouncements");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).ValueGeneratedNever();
            a.Property(x => x.OrganizationId).IsRequired();
            a.Property(x => x.RouteId).IsRequired();
            a.Property(x => x.AuthorUserId).IsRequired();
            a.Property(x => x.AuthorName).IsRequired().HasMaxLength(120);
            a.Property(x => x.Title).IsRequired().HasMaxLength(160);
            a.Property(x => x.Message).IsRequired().HasMaxLength(1200);
            a.Property(x => x.Priority).IsRequired().HasMaxLength(20);
            a.Property(x => x.CreatedAt).IsRequired();
        });

        builder.Entity<PickupChangeRequest>(p =>
        {
            p.ToTable("PickupChangeRequests");
            p.HasKey(x => x.Id);
            p.Property(x => x.Id).ValueGeneratedNever();
            p.Property(x => x.OrganizationId).IsRequired();
            p.Property(x => x.ParentId).IsRequired();
            p.Property(x => x.ChildId).IsRequired();
            p.Property(x => x.ChildName).IsRequired().HasMaxLength(120);
            p.Property(x => x.RouteId).IsRequired();
            p.Property(x => x.TargetDate).IsRequired().HasMaxLength(20);
            p.Property(x => x.NewAddress).IsRequired().HasMaxLength(250);
            p.Property(x => x.Reason).IsRequired().HasMaxLength(400);
            p.Property(x => x.Status).IsRequired().HasMaxLength(20);
            p.Property(x => x.CreatedAt).IsRequired();
        });

        builder.Entity<ChatMessage>(c =>
        {
            c.ToTable("ChatMessages");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).ValueGeneratedNever();
            c.Property(x => x.OrganizationId).IsRequired();
            c.Property(x => x.RouteId).IsRequired();
            c.Property(x => x.SenderUserId).IsRequired();
            c.Property(x => x.SenderName).IsRequired().HasMaxLength(120);
            c.Property(x => x.SenderRole).IsRequired().HasMaxLength(30);
            c.Property(x => x.Message).IsRequired().HasMaxLength(1200);
            c.Property(x => x.SentAt).IsRequired();
        });
    }

    /// <summary>
    ///     Use snake case naming convention for the database context
    /// </summary>
    /// <param name="builder">
    ///     The model builder for the database context
    /// </param>
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName)) entity.SetTableName(tableName.ToPlural().ToSnakeCase());


            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());


            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName)) key.SetName(keyName.ToSnakeCase());
            }


            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var foreignKeyName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(foreignKeyName)) foreignKey.SetConstraintName(foreignKeyName.ToSnakeCase());
            }


            foreach (var index in entity.GetIndexes())
            {
                var indexDatabaseName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexDatabaseName)) index.SetDatabaseName(indexDatabaseName.ToSnakeCase());
            }
        }
    }
}
