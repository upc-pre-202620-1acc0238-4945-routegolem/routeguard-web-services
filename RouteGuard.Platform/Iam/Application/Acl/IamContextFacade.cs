using RouteGuard.Platform.Iam.Application.QueryServices;
using RouteGuard.Platform.Iam.Domain.Model.Queries;
using RouteGuard.Platform.Iam.Interfaces.Acl;
using RouteGuard.Platform.Iam.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Iam.Application.Acl;

public class IamContextFacade(IUserQueryService userQueryService) : IIamContextFacade
{
    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return user is not null;
    }

    public async Task<string?> FetchUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return user?.Email.Value; 
    }

    /*
    public async Task<Guid?> FetchUserIdByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByEmailQuery(new Email(email)), cancellationToken);
        return user?.Id.Identifier;
    }
    */
}