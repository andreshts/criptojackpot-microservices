using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a user account is locked due to failed login attempts.
/// Consumed by: Notification microservice (sends security alert email)
/// </summary>
public class UserLockedOutEvent : Event
{
    public Guid UserGuid { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Number of failed attempts that triggered the lockout.
    /// </summary>
    public int FailedAttempts { get; set; }
    
    /// <summary>
    /// Duration of the lockout in minutes.
    /// </summary>
    public int LockoutMinutes { get; set; }
    
    /// <summary>
    /// IP address from the last failed attempt.
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent from the last failed attempt.
    /// </summary>
    public string? UserAgent { get; set; }
}

