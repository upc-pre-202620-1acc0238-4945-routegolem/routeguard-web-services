using Microsoft.AspNetCore.Authorization;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest;

public record RegisterDeviceTokenResource(string Token, string? Platform);

public record DeviceTokenResource(Guid Id, Guid UserId, string Platform, bool IsActive, DateTimeOffset RegisteredAt);

/// <summary>
///     Registers the push (FCM) token of a user's device (table <c>device_tokens</c>) so the
///     notifications of the user can be delivered as push messages.
/// </summary>
[ApiController]
[Route("api/v1/users/{userId:guid}/device-tokens")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Push device tokens of a user.")]
public class DeviceTokensController(AppDbContext context, CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Any)]
    [HttpPost]
    public async Task<IActionResult> Register(Guid userId, RegisterDeviceTokenResource resource,
        CancellationToken cancellationToken)
    {
        if (!caller.IsAdmin && caller.UserId != userId) return Forbid();

        if (string.IsNullOrWhiteSpace(resource.Token))
            return BadRequest(new { title = "The device token cannot be empty." });

        var id = new UserId(userId);
        if (!await context.Set<User>().AnyAsync(u => u.Id == id, cancellationToken)) return NotFound();

        var token = resource.Token.Trim();
        var platform = string.IsNullOrWhiteSpace(resource.Platform) ? "ANDROID" : resource.Platform.Trim().ToUpperInvariant();

        // A token belongs to one user: when the device is reused it is reassigned to the new user.
        var existing = await context.Set<DeviceToken>().FirstOrDefaultAsync(d => d.Token == token, cancellationToken);
        if (existing is not null && existing.UserId == id)
        {
            existing.Reactivate(platform);
        }
        else
        {
            if (existing is not null) context.Remove(existing);
            existing = new DeviceToken(id, token, platform);
            context.Add(existing);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Ok(ToResource(existing));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        if (!caller.IsAdmin && caller.UserId != userId) return Forbid();

        var id = new UserId(userId);
        var tokens = await context.Set<DeviceToken>().AsNoTracking().Where(d => d.UserId == id)
            .ToListAsync(cancellationToken);
        return Ok(tokens.Select(ToResource));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpDelete("{deviceTokenId:guid}")]
    public async Task<IActionResult> Deactivate(Guid userId, Guid deviceTokenId, CancellationToken cancellationToken)
    {
        if (!caller.IsAdmin && caller.UserId != userId) return Forbid();

        var id = new UserId(userId);
        var token = await context.Set<DeviceToken>()
            .FirstOrDefaultAsync(d => d.Id == deviceTokenId && d.UserId == id, cancellationToken);
        if (token is null) return NotFound();

        token.Deactivate();
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static DeviceTokenResource ToResource(DeviceToken d) =>
        new(d.Id, d.UserId.Identifier, d.Platform, d.IsActive, d.RegisteredAt);
}
