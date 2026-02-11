using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

/// <summary>
/// Finds a user by their Keycloak subject ID. If the user doesn't exist in the database
/// (e.g., self-registered via Keycloak), auto-provisions the user from their Keycloak profile.
/// </summary>
public class SyncCurrentUserQuery : IRequest<Result<UserDto>>
{
    /// <summary>
    /// Keycloak subject (sub claim / UUID).
    /// </summary>
    public string KeycloakId { get; set; } = null!;

    /// <summary>
    /// Email from token claims.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// First name from token claims.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name from token claims.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }
}
