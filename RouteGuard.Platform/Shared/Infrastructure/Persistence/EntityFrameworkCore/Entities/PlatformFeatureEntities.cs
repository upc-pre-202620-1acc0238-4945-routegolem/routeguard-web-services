namespace RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;

/// <summary>US-19: Temporary 6-digit password reset token persisted in PostgreSQL.</summary>
public class PasswordResetToken
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool Used { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Authorized family/caregiver contacts linked to a parent account.</summary>
public class AuthorizedCaregiver
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ParentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? PhotoData { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-23: User support ticket persisted in PostgreSQL.</summary>
public class SupportTicket
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "OPEN";
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-14: Parent service rating attached to a completed/cancelled trip.</summary>
public class TripRating
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TripId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid UserId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-09 & US-28: Parent early absence report linked to automatic route optimization.</summary>
public class AbsenceReport
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ParentId { get; set; }
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string AbsenceDate { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-20: Vehicle maintenance history record.</summary>
public class VehicleMaintenance
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Workshop { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int OdometerKm { get; set; }
    public string PerformedAt { get; set; } = string.Empty;
    public string NextDueDate { get; set; } = string.Empty;
    public string Status { get; set; } = "COMPLETED";
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-26: Operational fleet expense record (fuel, tolls, parking).</summary>
public class OperationalExpense
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string Category { get; set; } = "FUEL";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ExpenseDate { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-24: Driver/Admin announcement board post for a route.</summary>
public class RouteAnnouncement
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid RouteId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "NORMAL";
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-27: Temporary pickup/dropoff address change request from parent.</summary>
public class PickupChangeRequest
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ParentId { get; set; }
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string TargetDate { get; set; } = string.Empty;
    public string NewAddress { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>US-30: Internal route chat message between parent, driver and administrator.</summary>
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid RouteId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
}
