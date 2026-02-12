using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for authentication operations.
/// Encapsulates password verification, token generation, and login completion.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Verifies a password against the stored hash.
    /// </summary>
    bool VerifyPassword(string hash, string password);

    /// <summary>
    /// Completes the login process: generates tokens, saves refresh token, publishes event.
    /// </summary>
    Task<LoginResultDto> CompleteLoginAsync(
        User user,
        string? deviceInfo,
        string? ipAddress,
        bool rememberMe,
        CancellationToken cancellationToken);

    /// <summary>
    /// Handles 2FA login flow: generates challenge token.
    /// </summary>
    Task<LoginResultDto> HandleTwoFactorLoginAsync(
        User user,
        CancellationToken cancellationToken);

    /// <summary>
    /// Revokes a refresh token by its raw value (for logout).
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string rawToken, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the lockout duration in minutes based on failed attempts.
    /// </summary>
    int GetLockoutMinutes(int failedAttempts);
}
