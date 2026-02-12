using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a new user registers.
/// Consumed by: Notification microservice
/// </summary>
public class UserRegisteredEvent : Event
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// External GUID for cross-service communication
    /// </summary>
    public Guid UserGuid { get; set; }
    
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    
    /// <summary>
    /// Token for email verification. Empty/null if email is already verified.
    /// </summary>
    public string? ConfirmationToken { get; set; }
    
    /// <summary>
    /// True if user registered via Google OAuth (email pre-verified by Google).
    /// Consumer should skip sending verification email in this case.
    /// </summary>
    public bool IsExternalRegistration { get; set; }
    
    /// <summary>
    /// True if email is already verified (Google verified it or other means).
    /// </summary>
    public bool EmailVerified { get; set; }
}
