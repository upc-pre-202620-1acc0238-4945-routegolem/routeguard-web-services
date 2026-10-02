using RouteGuard.Platform.Iam.Domain.Model.Aggregates;

namespace RouteGuard.Platform.Iam.Application.Internal.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user);
}
