using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for refresh token operations with rotation support.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>Raw token (to be sent to client) - never store this directly</returns>
    string GenerateToken();

    /// <summary>
    /// Computes SHA-256 hash of a token for storage.
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Creates a new UserRefreshToken entity with proper hashing.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="familyId">Token family for rotation tracking (null for new family)</param>
    /// <param name="deviceInfo">Device fingerprint or User-Agent</param>
    /// <param name="ipAddress">Client IP</param>
    /// <param name="rememberMe">If true, extends expiration to 30 days</param>
    /// <returns>Tuple of (raw token for client, entity for storage)</returns>
    (string RawToken, UserRefreshToken Entity) CreateRefreshToken(
        long userId,
        Guid? familyId = null,
        string? deviceInfo = null,
        string? ipAddress = null,
        bool rememberMe = false);

    /// <summary>
    /// Validates a refresh token and returns the associated entity if valid.
    /// </summary>
    Task<UserRefreshToken?> ValidateAndGetTokenAsync(string rawToken);
}

