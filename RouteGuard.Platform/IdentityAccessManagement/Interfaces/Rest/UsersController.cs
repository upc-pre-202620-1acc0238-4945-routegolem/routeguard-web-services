using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Application.Internal.OutboundServices;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Entities;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.IdentityAccessManagement.Application.CommandServices;
using RouteGuard.Platform.IdentityAccessManagement.Application.QueryServices;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Commands;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Queries;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest.Resources;
using RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest.Transform;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.IdentityAccessManagement.Interfaces.Rest;

public record UpdateUserPhotoResource(string? PhotoData);
public record UpdateUserLanguageResource(string Language);
public record ForgotPasswordResource(string Email);
public record ResetPasswordResource(string Email, string Code, string NewPassword);

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Iam user accounts and authentication endpoints.")]
public class UsersController(
    IUserCommandService commandService,
    IUserQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory,
    CallerContext caller,
    AppDbContext dbContext,
    IHashingService hashingService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [SwaggerOperation(Summary = "Sign up", Description = "Registers a new user account.", OperationId = "SignUp")]
    public async Task<IActionResult> SignUp(SignUpResource resource, CancellationToken cancellationToken)
    {
        // Accounts are created by the administrator of the organization. The only anonymous sign-up allowed is the
        // first ADMIN of a newly registered organization (the "register administrator" flow of the app).
        if (caller.IsAdmin)
        {
            if (caller.OrganizationId is { } callerOrganization && resource.OrganizationId != callerOrganization)
                return Forbid();
        }
        else
        {
            var isFirstAdmin = string.Equals(resource.RoleTier, AppRoles.Admin, StringComparison.OrdinalIgnoreCase) &&
                               resource.OrganizationId is { } organizationId &&
                               !(await queryService.Handle(new GetAllUsersQuery(), cancellationToken))
                                   .Any(u => u.OrganizationId != null && u.OrganizationId.Identifier == organizationId);
            if (!isFirstAdmin) return caller.IsAuthenticated ? Forbid() : Unauthorized();
        }

        var result = await commandService.Handle(new SignUpCommand(resource.FirstName, resource.LastName,
            resource.Email, resource.Password, resource.RoleTier, resource.OrganizationId), cancellationToken);
        return IamActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            user => CreatedAtAction(nameof(GetUserById), new { userId = user.Id.Identifier },
                IamResourceFromEntityAssembler.ToResourceFromEntity(user)));
    }

    [AllowAnonymous]
    [HttpPost("sign-in")]
    [SwaggerOperation(Summary = "Sign in", Description = "Authenticates a user and issues a JWT.",
        OperationId = "SignIn")]
    public async Task<IActionResult> SignIn(SignInResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new SignInCommand(resource.Email, resource.Password),
            cancellationToken);
        return IamActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            authenticated => Ok(IamResourceFromEntityAssembler.ToAuthenticatedResourceFromEntity(
                authenticated.User, authenticated.Token)));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("{userId:guid}")]
    [SwaggerOperation(Summary = "Get a user by id", OperationId = "GetUserById")]
    public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        if (!caller.IsAdmin && caller.UserId != userId) return Forbid();

        var user = await queryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        if (user is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                IamError.UserNotFound, "User was not found.");
        return Ok(IamResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet]
    [SwaggerOperation(Summary = "Get all users", OperationId = "GetAllUsers")]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await queryService.Handle(new GetAllUsersQuery(), cancellationToken);
        return Ok(users.Select(IamResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get current signed-in user", OperationId = "GetCurrentUser")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        if (caller.UserId is not { } userId) return Unauthorized();
        var user = await queryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        if (user is null) return NotFound();
        return Ok(IamResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpPut("me/photo")]
    [SwaggerOperation(Summary = "Update current user profile photo", OperationId = "UpdateMyPhoto")]
    public async Task<IActionResult> UpdateMyPhoto(UpdateUserPhotoResource resource, CancellationToken cancellationToken)
    {
        if (caller.UserId is not { } userId) return Unauthorized();
        var user = await dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == new UserId(userId), cancellationToken);
        if (user is null) return NotFound();
        user.UpdatePhoto(resource.PhotoData);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(IamResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpPut("me/language")]
    [SwaggerOperation(Summary = "Update preferred UI language", OperationId = "UpdateMyLanguage")]
    public async Task<IActionResult> UpdateMyLanguage(UpdateUserLanguageResource resource, CancellationToken cancellationToken)
    {
        if (caller.UserId is not { } userId) return Unauthorized();
        var user = await dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == new UserId(userId), cancellationToken);
        if (user is null) return NotFound();
        user.SetPreferredLanguage(resource.Language);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(IamResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [SwaggerOperation(Summary = "Request password reset code (US-19)", OperationId = "ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordResource resource, CancellationToken cancellationToken)
    {
        var normalizedEmail = (resource.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return BadRequest(new { message = "Email is required." });

        var code = Random.Shared.Next(100000, 999999).ToString();
        var tokenEntity = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Code = code,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            Used = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await dbContext.Set<PasswordResetToken>().AddAsync(tokenEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            sent = true,
            email = normalizedEmail,
            expiresInMinutes = 15,
            verificationCode = code
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Reset password using code (US-19)", OperationId = "ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordResource resource, CancellationToken cancellationToken)
    {
        var normalizedEmail = (resource.Email ?? string.Empty).Trim().ToLowerInvariant();
        var code = (resource.Code ?? string.Empty).Trim();
        var newPassword = resource.NewPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(code) || newPassword.Length < 6)
            return BadRequest(new { message = "Valid email, verification code and password (>=6 chars) are required." });

        var token = await dbContext.Set<PasswordResetToken>()
            .Where(t => t.Email == normalizedEmail && t.Code == code && !t.Used && t.ExpiresAt >= DateTimeOffset.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (token is null)
            return BadRequest(new { message = "Invalid or expired reset code." });

        var user = await dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == new Domain.Model.ValueObjects.Email(normalizedEmail), cancellationToken);
        if (user is null)
            return NotFound(new { message = "User account was not found." });

        user.UpdatePasswordHash(new PasswordHash(hashingService.HashPassword(newPassword)));
        token.Used = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { reset = true, email = normalizedEmail });
    }
}
