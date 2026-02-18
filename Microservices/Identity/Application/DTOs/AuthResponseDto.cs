namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Response DTO for successful authentication.
/// Tokens are NOT included in the response body - they are set as HttpOnly cookies.
/// </summary>
public class AuthResponseDto
{
    public Guid UserGuid { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public string? Phone { get; set; }
    public string? ImagePath { get; set; }
    public bool Status { get; set; }
    public RoleDto? Role { get; set; }
    
    /// <summary>
    /// Access token expiration in seconds (for client-side refresh timing).
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Indicates if 2FA verification is required before issuing tokens.
    /// When true, a challenge token cookie is set instead of full auth cookies.
    /// </summary>
    public bool RequiresTwoFactor { get; set; }
    
    /// <summary>
    /// Indicates if the user has 2FA enabled (for UI display).
    /// </summary>
    public bool TwoFactorEnabled { get; set; }
}
