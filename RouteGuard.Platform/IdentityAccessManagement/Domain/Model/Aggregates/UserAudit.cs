using RouteGuard.Platform.Shared.Domain.Model.Entities;

namespace RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;

// This partial class injects auditory properties to the original User
public partial class User : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}