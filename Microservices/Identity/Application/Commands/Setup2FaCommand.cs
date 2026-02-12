using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to initiate 2FA setup.
/// Generates a new TOTP secret and QR code URI.
/// User must confirm with Confirm2FaCommand before 2FA is active.
/// </summary>
public class Setup2FaCommand : IRequest<Result<Setup2FaResultDto>>
{
    /// <summary>
    /// User GUID from JWT claims.
    /// </summary>
    public Guid UserGuid { get; set; }
}

