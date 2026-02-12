using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a security threat is detected.
/// Examples: refresh token reuse (theft detection), suspicious login patterns.
/// Consumed by: Notification microservice (sends security alert email)
/// </summary>
public class SecurityAlertEvent : Event
{
    public Guid UserGuid { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Type of security alert.
    /// </summary>
    public SecurityAlertType AlertType { get; set; }
    
    /// <summary>
    /// Human-readable description of the alert.
    /// </summary>
    public string Description { get; set; } = null!;
    
    /// <summary>
    /// IP address associated with the suspicious activity.
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent associated with the suspicious activity.
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Additional context (JSON serialized).
    /// </summary>
    public string? AdditionalContext { get; set; }
}


