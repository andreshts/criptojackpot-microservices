using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to authenticate or register a user via Google OAuth.
/// Handles three scenarios:
/// 1. Existing user with GoogleId → Login
/// 2. Existing user by email without GoogleId → Link Google account
/// 3. New user → Register with Google
/// </summary>
public class GoogleLoginCommand : IRequest<Result<LoginResultDto>>
{
    /// <summary>
    /// Google ID token from the OAuth flow.
    /// Validated against Google's public keys.
    /// </summary>
    public string IdToken { get; set; } = null!;

    /// <summary>
    /// Google access token (optional, for API access).
    /// Will be encrypted before storage.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Google refresh token (optional, only provided on first consent).
    /// Will be encrypted before storage.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time in seconds (from Google).
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Client IP address for audit and refresh token.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Device info/User-Agent for session tracking.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Remember me preference for session duration.
    /// </summary>
    public bool RememberMe { get; set; }
}

