using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Iam.Application.CommandServices;
using RouteGuard.Platform.Iam.Application.Internal.OutboundServices;
using RouteGuard.Platform.Iam.Domain.Model;
using RouteGuard.Platform.Iam.Domain.Model.Aggregates;
using RouteGuard.Platform.Iam.Domain.Model.Commands;
using RouteGuard.Platform.Iam.Domain.Model.ValueObjects;
using RouteGuard.Platform.Iam.Domain.Repositories;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;

namespace RouteGuard.Platform.Iam.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IHashingService hashingService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IUserCommandService
{
    public async Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
            return Result<User>.Failure(IamError.EmailAlreadyRegistered, $"Email {command.Email} is already registered.");

        try
        {
            var organizationId = command.OrganizationId.HasValue ? new OrganizationId(command.OrganizationId.Value) : null;
            var user = new User(organizationId, new FullName(command.FirstName, command.LastName), new Email(command.Email), new PasswordHash(hashingService.HashPassword(command.Password)), new RoleTier(command.RoleTier));

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<User>.Success(user);
        }
        catch (ArgumentException ex) { return Result<User>.Failure(IamError.InvalidIamData, ex.Message); }
        catch (DbUpdateException) { return Result<User>.Failure(IamError.DatabaseError, "A database error occurred."); }
    }

    public async Task<Result<(User User, string Token)>> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (user is null || !hashingService.VerifyPassword(command.Password, user.PasswordHash.Value))
            return Result<(User User, string Token)>.Failure(IamError.InvalidCredentials, "Invalid email or password.");

        var token = tokenService.GenerateToken(user);
        return Result<(User User, string Token)>.Success((user, token));
    }
}