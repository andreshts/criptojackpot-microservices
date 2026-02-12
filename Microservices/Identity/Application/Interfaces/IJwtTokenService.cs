using System.Security.Claims;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token (short-lived JWT) for the user.
    /// </summary>
    /// <param name="user">User entity with role</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    [Obsolete("Use GenerateAccessToken(User user) instead")]
    string GenerateToken(string userId);

    /// <summary>
    /// Validates a JWT and extracts the claims principal.
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts the UserGuid from claims.
    /// </summary>
    Guid? GetUserGuidFromClaims(ClaimsPrincipal principal);
}

