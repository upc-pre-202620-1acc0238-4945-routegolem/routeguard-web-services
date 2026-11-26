using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.StakeholderAssetManagement.Interfaces.Rest;

public record CreateCaregiverResource(string FullName, string Relationship, string PhoneNumber, string? DocumentNumber, string? PhotoData);

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Authorized family caregivers persisted in PostgreSQL.")]
public class CaregiversController(AppDbContext context, CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Any)]
    [HttpGet]
    [SwaggerOperation("Get Authorized Caregivers", "Returns authorized caregivers for the current parent or organization.")]
    public async Task<IActionResult> GetCaregivers([FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        var resolvedParentId = parentId;
        if (caller.IsParent)
            resolvedParentId = await caller.GetParentIdAsync(cancellationToken);

        var query = context.Set<AuthorizedCaregiver>().AsNoTracking();
        if (resolvedParentId.HasValue)
            query = query.Where(c => c.ParentId == resolvedParentId.Value);
        else if (caller.OrganizationId is { } orgId)
            query = query.Where(c => c.OrganizationId == orgId);

        var items = await query.OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.AdminOrParent)]
    [HttpPost]
    [SwaggerOperation("Add Authorized Caregiver", "Registers a new authorized caregiver for child pickup.")]
    public async Task<IActionResult> AddCaregiver(CreateCaregiverResource resource, [FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.FullName) || string.IsNullOrWhiteSpace(resource.PhoneNumber))
            return BadRequest(new { message = "FullName and PhoneNumber are required." });

        var resolvedParentId = caller.IsParent
            ? await caller.GetParentIdAsync(cancellationToken)
            : parentId;

        if (!resolvedParentId.HasValue || resolvedParentId.Value == Guid.Empty)
            resolvedParentId = await context.Set<RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates.Parent>()
                .Select(p => p.Id.Identifier)
                .FirstOrDefaultAsync(cancellationToken);

        var entity = new AuthorizedCaregiver
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding.IamSeeder.SeedOrganizationId,
            ParentId = resolvedParentId ?? Guid.Empty,
            FullName = resource.FullName.Trim(),
            Relationship = string.IsNullOrWhiteSpace(resource.Relationship) ? "Familiar autorizado" : resource.Relationship.Trim(),
            PhoneNumber = resource.PhoneNumber.Trim(),
            DocumentNumber = resource.DocumentNumber?.Trim(),
            PhotoData = resource.PhotoData?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<AuthorizedCaregiver>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/caregivers/{entity.Id}", entity);
    }

    [Authorize(Roles = AppRoles.AdminOrParent)]
    [HttpDelete("{caregiverId:guid}")]
    [SwaggerOperation("Delete Authorized Caregiver", "Removes an authorized caregiver.")]
    public async Task<IActionResult> DeleteCaregiver(Guid caregiverId, CancellationToken cancellationToken)
    {
        var item = await context.Set<AuthorizedCaregiver>().FirstOrDefaultAsync(c => c.Id == caregiverId, cancellationToken);
        if (item is null) return NotFound();

        if (caller.IsParent && item.ParentId != await caller.GetParentIdAsync(cancellationToken))
            return Forbid();

        context.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
