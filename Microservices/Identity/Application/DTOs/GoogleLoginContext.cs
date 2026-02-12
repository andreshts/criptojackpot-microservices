namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Context data for Google login operations.
/// Groups request metadata to reduce method parameters.
/// </summary>
public record GoogleLoginContext
{
    /// <summary>
    /// Google access token (optional, for API access).
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Google refresh token (optional, only on first consent).
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Token expiration in seconds.
    /// </summary>
    public int? ExpiresIn { get; init; }

    /// <summary>
    /// Device info/User-Agent.
    /// </summary>
    public string? DeviceInfo { get; init; }

    /// <summary>
    /// Client IP address.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Remember me preference.
    /// </summary>
    public bool RememberMe { get; init; }
}

