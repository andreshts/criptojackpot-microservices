using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to confirm 2FA setup by validating TOTP code.
/// If successful, enables 2FA and generates recovery codes.
/// </summary>
public class Confirm2FaCommand : IRequest<Result<Confirm2FaResultDto>>
{
    /// <summary>
    /// User GUID from JWT claims.
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// 6-digit TOTP code from authenticator app to verify setup.
    /// </summary>
    public string Code { get; set; } = null!;
}

