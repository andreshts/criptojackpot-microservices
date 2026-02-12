namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Configuration for authentication cookies.
/// Bind to "CookieSettings" section in appsettings.json.
/// </summary>
/// <remarks>
/// Cookie prefix rules (RFC 6265bis):
/// - __Host-: Requires Secure=true, Path=/, and NO Domain attribute
/// - __Secure-: Requires Secure=true, allows Domain attribute
/// 
/// Since we support Domain configuration, we use __Secure- prefix in production
/// and no prefix in development.
/// </remarks>
public class CookieConfig
{
    public const string SectionName = "CookieSettings";

    /// <summary>
    /// Domain for cookies (e.g., ".cryptojackpot.com" for sharing across subdomains).
    /// Leave null to use the request domain (recommended for development).
    /// WARNING: If Domain is set, __Host- prefix CANNOT be used.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Whether cookies require HTTPS. Should be true in production.
    /// </summary>
    public bool SecureOnly { get; set; } = true;

    /// <summary>
    /// SameSite policy for cookies.
    /// Use "Strict" for same-site only, "Lax" for top-level navigations, "None" for cross-site (requires SecureOnly=true).
    /// </summary>
    public string SameSite { get; set; } = "Strict";

    /// <summary>
    /// Access token cookie name.
    /// Use "__Secure-" prefix in production (requires SecureOnly=true).
    /// </summary>
    public string AccessTokenCookieName { get; set; } = "access_token";

    /// <summary>
    /// Refresh token cookie name.
    /// Use "__Secure-" prefix in production (requires SecureOnly=true).
    /// </summary>
    public string RefreshTokenCookieName { get; set; } = "refresh_token";

    /// <summary>
    /// Path for auth cookies. Usually "/" for site-wide access.
    /// </summary>
    public string Path { get; set; } = "/";
}

