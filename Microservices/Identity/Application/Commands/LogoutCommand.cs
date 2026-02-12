using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command for user logout. Revokes the refresh token.
/// </summary>
public class LogoutCommand : IRequest<Result>
{
    /// <summary>
    /// The raw refresh token from the HttpOnly cookie.
    /// </summary>
    public string? RefreshToken { get; set; }
}

