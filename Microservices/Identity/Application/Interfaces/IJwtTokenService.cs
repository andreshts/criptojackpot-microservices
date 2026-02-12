using System.Security.Claims;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token (short-lived JWT) for the user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a restricted challenge token for 2FA verification.
    /// This token has "purpose": "2fa_challenge" claim and NO role claims.
    /// It cannot be used to access protected endpoints.
    /// </summary>
    string GenerateTwoFactorChallengeToken(User user, int expiresInMinutes = 5);

    /// <summary>
    /// Validates a 2FA challenge token and returns the user GUID if valid.
    /// </summary>
    Guid? ValidateTwoFactorChallengeToken(string token);
    
    /// <summary>
    /// Validates a JWT and extracts the claims principal.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts the UserGuid from claims.
    /// </summary>
    Guid? GetUserGuidFromClaims(ClaimsPrincipal principal);
}

