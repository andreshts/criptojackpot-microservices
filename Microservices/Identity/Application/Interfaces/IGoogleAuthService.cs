using CryptoJackpot.Identity.Application.DTOs;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for Google OAuth token validation and user info retrieval.
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Validates a Google ID token and extracts the payload.
    /// </summary>
    /// <param name="idToken">The ID token from Google OAuth flow</param>
    /// <returns>Validated payload or null if invalid</returns>
    Task<GoogleUserPayload?> ValidateIdTokenAsync(string idToken);
}


