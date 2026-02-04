using CryptoJackpot.Identity.Application.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Interface for user token operations (login, refresh, logout).
/// Uses Resource Owner Password Credentials flow for user authentication.
/// </summary>
public interface IKeycloakTokenService
{
    /// <summary>
    /// Exchanges credentials for tokens using the Resource Owner Password Credentials flow.
    /// </summary>
    Task<KeycloakTokenResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    Task<KeycloakTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a token (logout at token level).
    /// </summary>
    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
