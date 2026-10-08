using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Repositories;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public Task<User?> FindByUserIdAsync(UserId userId, CancellationToken cancellationToken) =>
        Context.Set<User>().FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = new Email(email, false);
        return Context.Set<User>().FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = new Email(email, false);
        return Context.Set<User>().AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
    }
}
