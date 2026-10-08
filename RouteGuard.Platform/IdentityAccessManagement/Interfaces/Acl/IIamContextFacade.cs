namespace RouteGuard.Platform.IdentityAccessManagement.Interfaces.Acl;

/// <summary>
/// Fachada para el contexto IAM. Único punto de entrada permitido 
/// para que otros Bounded Contexts (como Fleet o Trip) consulten datos de usuarios.
/// </summary>
public interface IIamContextFacade
{
    // Stakeholder asks: "Is there an user with this ID to sync with a Driver?"
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    // Notifications asks: "I have this ID of father/mother, which is their address so I can be able to email them?"
    Task<string?> FetchUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken);
    // Fleet asks: "I want to invite a user via email, give me its ID"
    // Task<Guid?> FetchUserIdByEmailAsync(string email, CancellationToken cancellationToken);
}