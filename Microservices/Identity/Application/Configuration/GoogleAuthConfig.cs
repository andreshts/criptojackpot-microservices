namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Configuration for Google OAuth authentication.
/// Bind to "GoogleAuth" section in appsettings.json.
/// </summary>
public class GoogleAuthConfig
{
    public const string SectionName = "GoogleAuth";
    
    /// <summary>
    /// Google OAuth Client ID from Google Cloud Console.
    /// </summary>
    public string ClientId { get; set; } = null!;
    
    /// <summary>
    /// Google OAuth Client Secret from Google Cloud Console.
    /// Should be stored in secrets/vault in production.
    /// </summary>
    public string ClientSecret { get; set; } = null!;
}

