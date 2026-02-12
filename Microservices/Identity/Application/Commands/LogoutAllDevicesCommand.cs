using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to revoke all refresh tokens for a user (logout from all devices).
/// Use when user suspects account compromise or wants to end all sessions.
/// </summary>
public class LogoutAllDevicesCommand : IRequest<Result<int>>
{
    /// <summary>
    /// User GUID from JWT claims.
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Reason for the logout (for audit purposes).
    /// </summary>
    public string? Reason { get; set; }
}

