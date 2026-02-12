using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for Google OAuth login flow.
/// Handles user lookup/creation and token storage.
/// </summary>
public interface IGoogleLoginService
{
    /// <summary>
    /// Authenticates or registers a user via Google OAuth.
    /// </summary>
    /// <param name="payload">Validated Google ID token payload</param>
    /// <param name="context">Login context with tokens and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login result with tokens, or error</returns>
    Task<Result<LoginResultDto>> LoginOrRegisterAsync(
        GoogleUserPayload payload,
        GoogleLoginContext context,
        CancellationToken cancellationToken);
}

