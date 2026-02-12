namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Configuration for JWT token generation and validation.
/// Bind to "JwtSettings" section in appsettings.json.
/// </summary>
public class JwtConfig
{
    public const string SectionName = "JwtSettings";
    
    /// <summary>
    /// Secret key for signing JWT tokens (minimum 256 bits / 32 characters).
    /// Should be stored in secrets/vault in production.
    /// </summary>
    public string SecretKey { get; set; } = null!;
    
    /// <summary>
    /// Token issuer (typically your API domain).
    /// </summary>
    public string Issuer { get; set; } = null!;
    
    /// <summary>
    /// Token audience (typically your frontend domain).
    /// </summary>
    public string Audience { get; set; } = null!;
    
    /// <summary>
    /// Access token expiration in minutes.
    /// Recommended: 15 minutes for security.
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 15;
}

