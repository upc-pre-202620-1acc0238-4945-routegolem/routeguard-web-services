using RouteGuard.Platform.Iam.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Model.Entities;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Notifications.Domain.Model.Entities;

/// <summary>Push (FCM) token of a user's device (table <c>device_tokens</c>).</summary>
public class DeviceToken : IAuditableEntity
{
    protected DeviceToken()
    {
        UserId = new UserId(Guid.Empty);
    }

    public DeviceToken(UserId userId, string token, string platform)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        Platform = platform;
        IsActive = true;
        RegisteredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string Platform { get; private set; } = "ANDROID";
    public bool IsActive { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void Reactivate(string platform)
    {
        Platform = platform;
        IsActive = true;
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}
