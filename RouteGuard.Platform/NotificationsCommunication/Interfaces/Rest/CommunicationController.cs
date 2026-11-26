using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using Swashbuckle.AspNetCore.Annotations;
using RouteAggregate = RouteGuard.Platform.FleetRouteManagement.Domain.Model.Aggregates.Route;
using ParentAggregate = RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates.Parent;

namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest;

public record CreateRouteAnnouncementResource(Guid? RouteId, string Title, string Message, string? Priority);
public record CreatePickupChangeResource(Guid? ChildId, string ChildName, Guid? RouteId, string? TargetDate, string NewAddress, double? Latitude, double? Longitude, string Reason);
public record UpdatePickupChangeStatusResource(string Status);
public record CreateChatMessageResource(Guid? RouteId, string Message);

[ApiController]
[Route("api/v1")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Driver announcements board (US-24), temporary pickup changes (US-27), and internal route chat (US-30).")]
public class CommunicationController(AppDbContext context, CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("announcements")]
    [SwaggerOperation("Get Driver Announcements (US-24)", "Returns announcements published for routes in the organization.")]
    public async Task<IActionResult> GetAnnouncements([FromQuery] Guid? routeId, CancellationToken cancellationToken)
    {
        var query = context.Set<RouteAnnouncement>().AsNoTracking();
        if (routeId.HasValue)
            query = query.Where(a => a.RouteId == routeId.Value);
        else if (caller.OrganizationId is { } orgId)
            query = query.Where(a => a.OrganizationId == orgId);

        var items = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPost("announcements")]
    [SwaggerOperation("Publish Route Announcement (US-24)", "Publishes a driver or admin announcement for parents on the route.")]
    public async Task<IActionResult> CreateAnnouncement(CreateRouteAnnouncementResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Title) || string.IsNullOrWhiteSpace(resource.Message))
            return BadRequest(new { message = "Title and Message are required." });

        var routeId = resource.RouteId ?? await context.Set<RouteAggregate>().Select(r => r.Id.Identifier).FirstOrDefaultAsync(cancellationToken);
        var authorName = await ResolveCallerNameAsync(cancellationToken);

        var entity = new RouteAnnouncement
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            RouteId = routeId,
            AuthorUserId = caller.UserId ?? Guid.Empty,
            AuthorName = authorName,
            Title = resource.Title.Trim(),
            Message = resource.Message.Trim(),
            Priority = string.IsNullOrWhiteSpace(resource.Priority) ? "NORMAL" : resource.Priority.Trim().ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<RouteAnnouncement>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/announcements/{entity.Id}", entity);
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("pickup-changes")]
    [SwaggerOperation("Get Temporary Pickup Changes (US-27)", "Returns temporary pickup address requests.")]
    public async Task<IActionResult> GetPickupChanges(CancellationToken cancellationToken)
    {
        var query = context.Set<PickupChangeRequest>().AsNoTracking();
        if (caller.IsParent)
        {
            var parentId = await caller.GetParentIdAsync(cancellationToken);
            if (parentId.HasValue) query = query.Where(p => p.ParentId == parentId.Value);
        }
        else if (caller.OrganizationId is { } orgId)
        {
            query = query.Where(p => p.OrganizationId == orgId);
        }

        var items = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.AdminOrParent)]
    [HttpPost("pickup-changes")]
    [SwaggerOperation("Request Temporary Pickup Change (US-27)", "Submits a parent request for a temporary pickup address change.")]
    public async Task<IActionResult> CreatePickupChange(CreatePickupChangeResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.ChildName) || string.IsNullOrWhiteSpace(resource.NewAddress))
            return BadRequest(new { message = "ChildName and NewAddress are required." });

        var parentId = caller.IsParent ? await caller.GetParentIdAsync(cancellationToken) : null;
        var parent = parentId.HasValue
            ? await context.Set<ParentAggregate>().FirstOrDefaultAsync(p => p.Id == new RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.ParentId(parentId.Value), cancellationToken)
            : await context.Set<ParentAggregate>().FirstOrDefaultAsync(cancellationToken);
        var routeId = resource.RouteId ?? await context.Set<RouteAggregate>().Select(r => r.Id.Identifier).FirstOrDefaultAsync(cancellationToken);

        var entity = new PickupChangeRequest
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            ParentId = parent?.Id.Identifier ?? Guid.Empty,
            ChildId = resource.ChildId ?? parent?.Children.FirstOrDefault()?.Id.Identifier ?? Guid.Empty,
            ChildName = resource.ChildName.Trim(),
            RouteId = routeId,
            TargetDate = string.IsNullOrWhiteSpace(resource.TargetDate) ? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd") : resource.TargetDate.Trim(),
            NewAddress = resource.NewAddress.Trim(),
            Latitude = resource.Latitude,
            Longitude = resource.Longitude,
            Reason = resource.Reason?.Trim() ?? string.Empty,
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<PickupChangeRequest>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/pickup-changes/{entity.Id}", entity);
    }

    [Authorize(Roles = AppRoles.AdminOrDriver)]
    [HttpPatch("pickup-changes/{requestId:guid}/status")]
    [SwaggerOperation("Review Temporary Pickup Change (US-27)", "Approves or rejects a temporary pickup change request.")]
    public async Task<IActionResult> UpdatePickupChangeStatus(Guid requestId, UpdatePickupChangeStatusResource resource, CancellationToken cancellationToken)
    {
        var entity = await context.Set<PickupChangeRequest>().FirstOrDefaultAsync(p => p.Id == requestId, cancellationToken);
        if (entity is null) return NotFound();

        var normalized = (resource.Status ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized != "APPROVED" && normalized != "REJECTED" && normalized != "PENDING")
            return BadRequest(new { message = "Status must be APPROVED, REJECTED or PENDING." });

        entity.Status = normalized;
        await context.SaveChangesAsync(cancellationToken);
        return Ok(entity);
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("chat")]
    [SwaggerOperation("Get Internal Route Chat (US-30)", "Returns chat messages between driver and parents.")]
    public async Task<IActionResult> GetChatMessages([FromQuery] Guid? routeId, CancellationToken cancellationToken)
    {
        var query = context.Set<ChatMessage>().AsNoTracking();
        if (routeId.HasValue)
            query = query.Where(c => c.RouteId == routeId.Value);
        else if (caller.OrganizationId is { } orgId)
            query = query.Where(c => c.OrganizationId == orgId);

        var items = await query.OrderBy(c => c.SentAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpPost("chat")]
    [SwaggerOperation("Send Internal Chat Message (US-30)", "Persists an internal chat message between parent and driver.")]
    public async Task<IActionResult> SendChatMessage(CreateChatMessageResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Message))
            return BadRequest(new { message = "Message is required." });

        var routeId = resource.RouteId ?? await context.Set<RouteAggregate>().Select(r => r.Id.Identifier).FirstOrDefaultAsync(cancellationToken);
        var senderName = await ResolveCallerNameAsync(cancellationToken);

        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            RouteId = routeId,
            SenderUserId = caller.UserId ?? Guid.Empty,
            SenderName = senderName,
            SenderRole = caller.Role ?? AppRoles.Parent,
            Message = resource.Message.Trim(),
            SentAt = DateTimeOffset.UtcNow
        };

        await context.Set<ChatMessage>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/chat/{entity.Id}", entity);
    }

    private async Task<string> ResolveCallerNameAsync(CancellationToken cancellationToken)
    {
        if (caller.UserId is { } uid)
        {
            var user = await context.Set<User>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == new UserId(uid), cancellationToken);
            if (user is not null) return user.FullName.ToString();
        }
        return caller.Email ?? "Usuario";
    }
}
