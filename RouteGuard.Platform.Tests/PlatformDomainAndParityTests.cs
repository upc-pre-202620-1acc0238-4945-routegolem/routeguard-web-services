using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Aggregates;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;
using RouteGuard.Platform.TripExecutionMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using Xunit;
using TripOrgId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.OrganizationId;
using TripRouteId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.RouteId;
using TripDriverId = RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects.DriverId;

namespace RouteGuard.Platform.Tests;

public class PlatformDomainAndParityTests
{
    [Fact]
    public void User_ShouldPersistPhotoAndPreferredLanguage()
    {
        var user = new User(
            IdentityAccessManagement.Domain.Model.ValueObjects.OrganizationId.New(),
            new FullName("Rosita", "Nery"),
            new Email("rosita@routeguard.pe"),
            new PasswordHash("hashed"),
            new RoleTier(RoleTier.Parent));

        user.UpdatePhoto("data:image/jpeg;base64,AAAA");
        user.SetPreferredLanguage("EN");

        Assert.Equal("data:image/jpeg;base64,AAAA", user.PhotoData);
        Assert.Equal("en", user.PreferredLanguage);
    }

    [Fact]
    public void Plan_UpdateCatalog_ShouldAlignUnifiedSolesPricing()
    {
        var plan = new Plan(new PlanTier(PlanTier.Basic), new RouteQuota(2), new DriverQuota(2), 9.99m);
        plan.UpdateCatalog(2, 2, 49.99m);

        Assert.Equal(49.99m, plan.Price);
        Assert.Equal(2, plan.GetRouteLimit());
    }

    [Fact]
    public void VehicleCatalogItem_ShouldUpdateSoatDetails()
    {
        var vehicle = new VehicleCatalogItem
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Plate = "ABC-123",
            Model = "Sprinter 416",
            Capacity = 20
        };

        vehicle.Update("ABC-123", "Mercedes-Benz Sprinter 416", 20, "ACTIVE",
            "Mercedes-Benz", "SOAT-2026-88412", "Rímac Seguros", "2026-12-15", true);

        Assert.Equal("SOAT-2026-88412", vehicle.SoatPolicy);
        Assert.Equal("Rímac Seguros", vehicle.SoatInsurer);
        Assert.True(vehicle.SoatValid);
    }

    [Fact]
    public void Trip_Cancel_ShouldTransitionToCancelled()
    {
        var trip = new Trip(TripOrgId.New(), TripRouteId.New(), TripDriverId.New());
        trip.Start();
        trip.Cancel();

        Assert.True(trip.State.IsCancelled());
        Assert.NotNull(trip.CancelledAt);
    }

    [Theory]
    [InlineData(-12.0464, -77.0428, true)]  // Colegio San Martin, Lima
    [InlineData(-12.1211, -77.0297, true)]  // Parque Kennedy, Miraflores
    [InlineData(-12.0776, -77.0824, true)]  // Plaza San Miguel, Lima
    [InlineData(37.421998, -122.084, false)] // Android Emulator Mountain View
    [InlineData(40.7128, -74.0060, false)]   // New York
    [InlineData(0.0, 0.0, false)]            // Null Island
    public void TripSeeder_IsWithinLimaMetro_ShouldFilterOutNonLimaCoordinates(
        double lat, double lng, bool expected)
    {
        Assert.Equal(expected, TripSeeder.IsWithinLimaMetro(lat, lng));
    }

    [Fact]
    public void LocationRecord_Relocate_ShouldMoveStrayCoordinateOntoRouteAndKeepTimestamp()
    {
        var recorded = DateTimeOffset.UtcNow.AddMinutes(-10);
        var recordId = Guid.NewGuid();
        var record = new TripExecutionMonitoring.Domain.Model.Entities.LocationRecord(
            recordId, new TripId(Guid.NewGuid()), 40.7128, -74.0060, 0, 95, 180, recorded);

        record.Relocate(-12.1211, -77.0297, 28.5);

        Assert.Equal(recordId, record.Id);
        Assert.Equal(recorded, record.RecordedAt);
        Assert.True(TripSeeder.IsWithinLimaMetro(record.Latitude, record.Longitude));
        Assert.Equal(28.5, record.SpeedKmh);
    }

    [Fact]
    public void Subscription_Renew_ShouldExtendEndDateAndRemainActive()
    {
        var start = DateTimeOffset.UtcNow.AddDays(-40);
        var sub = new Subscription(
            new SubscriptionPlanManagement.Domain.Model.ValueObjects.OrganizationId(Guid.NewGuid()),
            new PlanId(Guid.NewGuid()),
            start,
            start.AddDays(30));

        Assert.Equal(0, sub.GetRemainingDays());

        sub.Renew(DateTimeOffset.UtcNow.AddDays(30));

        Assert.True(sub.IsActive());
        Assert.True(sub.GetRemainingDays() >= 29);
    }
}
