using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.Internal.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user);
}
