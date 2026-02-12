namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request for Google OAuth login.
/// </summary>
public class GoogleLoginRequest
{
    /// <summary>
    /// Google ID token from OAuth flow.
    /// </summary>
    public string IdToken { get; set; } = null!;

    /// <summary>
    /// Google access token (optional).
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Google refresh token (optional, only on first consent).
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration in seconds.
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Remember me preference.
    /// </summary>
    public bool RememberMe { get; set; }
}

