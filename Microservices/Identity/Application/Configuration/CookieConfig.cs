namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Configuration for authentication cookies.
/// Bind to "CookieSettings" section in appsettings.json.
/// </summary>
public class CookieConfig
{
    public const string SectionName = "CookieSettings";

    /// <summary>
    /// Domain for cookies (e.g., ".cryptojackpot.com" for sharing across subdomains).
    /// Leave null in development to use the request domain.
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
    /// </summary>
    public string AccessTokenCookieName { get; set; } = "__Host-access_token";

    /// <summary>
    /// Refresh token cookie name.
    /// </summary>
    public string RefreshTokenCookieName { get; set; } = "__Host-refresh_token";

    /// <summary>
    /// Path for auth cookies. Usually "/" for site-wide access.
    /// </summary>
    public string Path { get; set; } = "/";
}

