using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using StakeholderEmail = RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.Email;

namespace RouteGuard.Platform.Shared.Interfaces.Rest.Security;

/// <summary>
///     Who is calling the API: the user, role and organization from the JWT, plus the driver / parent
///     profile that belongs to that user. Controllers use it to restrict access to "what is mine".
/// </summary>
public class CallerContext(IHttpContextAccessor accessor, AppDbContext context)
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public string? Role => User?.FindFirstValue(ClaimTypes.Role);
    public bool IsAdmin => Role == AppRoles.Admin;
    public bool IsDriver => Role == AppRoles.Driver;
    public bool IsParent => Role == AppRoles.Parent;
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public Guid? OrganizationId =>
        Guid.TryParse(User?.FindFirstValue("organizationId"), out var id) ? id : null;

    /// <summary>Id of the driver profile of the caller (matched by the e-mail of the account), if any.</summary>
    public async Task<Guid?> GetDriverIdAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Email)) return null;
        var email = new StakeholderEmail(Email);
        var driver = await context.Set<Driver>().AsNoTracking()
            .FirstOrDefaultAsync(d => d.Email == email, cancellationToken);
        return driver?.Id.Identifier;
    }

    /// <summary>Id of the parent profile of the caller (matched by the e-mail of the account), if any.</summary>
    public async Task<Guid?> GetParentIdAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Email)) return null;
        var email = new StakeholderEmail(Email);
        var parent = await context.Set<Parent>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        return parent?.Id.Identifier;
    }
}
