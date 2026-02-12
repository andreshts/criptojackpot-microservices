using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to disable 2FA for a user.
/// Requires TOTP code or recovery code for verification.
/// </summary>
public class Disable2FaCommand : IRequest<Result>
{
    /// <summary>
    /// User GUID from JWT claims.
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// 6-digit TOTP code from authenticator app.
    /// Either Code or RecoveryCode must be provided.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Recovery code if authenticator is unavailable.
    /// </summary>
    public string? RecoveryCode { get; set; }
}

