using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest;

public record CreateSupportTicketResource(string Category, string Subject, string Message);

[ApiController]
[Route("api/v1/support-tickets")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Help center support tickets (US-23).")]
public class SupportTicketsController(AppDbContext context, CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Any)]
    [HttpGet]
    [SwaggerOperation("Get Support Tickets", "Returns support tickets submitted by the caller or organization.")]
    public async Task<IActionResult> GetTickets(CancellationToken cancellationToken)
    {
        var query = context.Set<SupportTicket>().AsNoTracking();
        if (!caller.IsAdmin && caller.UserId is { } userId)
            query = query.Where(t => t.UserId == userId);
        else if (caller.OrganizationId is { } orgId)
            query = query.Where(t => t.OrganizationId == orgId);

        var items = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpPost]
    [SwaggerOperation("Create Support Ticket", "Creates a support ticket in PostgreSQL (US-23).")]
    public async Task<IActionResult> CreateTicket(CreateSupportTicketResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Subject) || string.IsNullOrWhiteSpace(resource.Message))
            return BadRequest(new { message = "Subject and Message are required." });

        var code = $"#RG-{Random.Shared.Next(1000, 9999)}";
        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            OrganizationId = caller.OrganizationId ?? IamSeeder.SeedOrganizationId,
            UserId = caller.UserId ?? Guid.Empty,
            TicketCode = code,
            Category = string.IsNullOrWhiteSpace(resource.Category) ? "GENERAL" : resource.Category.Trim(),
            Subject = resource.Subject.Trim(),
            Message = resource.Message.Trim(),
            Status = "OPEN",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Set<SupportTicket>().AddAsync(ticket, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/support-tickets/{ticket.Id}", ticket);
    }
}
