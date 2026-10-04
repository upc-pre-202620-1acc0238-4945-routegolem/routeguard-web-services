using RouteGuard.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Trip.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using RouteGuard.Platform.Fleet.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using RouteGuard.Platform.Notifications.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using RouteGuard.Platform.Stakeholder.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using RouteGuard.Platform.Subscription.Infraestructure.Persistence.EntityFrameworkCore.Configuration.Extensions;


namespace RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;


/// <summary>
///     Application database context for the Learning Center Platform
/// </summary>
/// <param name="options">
///     The options for the database context
/// </param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Apply audit timestamp interceptor for all IAuditableEntity implementations
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }


    /// <summary>
    ///     On creating the database model
    /// </summary>
    /// <remarks>
    ///     This method is used to create the database model for the application.
    /// </remarks>
    /// <param name="builder">
    ///     The model builder for the database context
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Per-bounded-context model configuration (declared in each context's Infrastructure layer).
        builder.ApplyIamConfiguration();
        builder.ApplyStakeholderConfiguration();
        builder.ApplyTripConfiguration();
        builder.ApplyFleetConfiguration();
        builder.ApplySubscriptionConfiguration();
        builder.ApplyNotificationConfiguration();
        builder.ApplySharedConfiguration();

        // General Naming Convention for the database objects
        builder.UseSnakeCaseNamingConvention();
    }
}
