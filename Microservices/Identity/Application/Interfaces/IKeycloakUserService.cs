using CryptoJackpot.Identity.Application.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Interface for CRUD operations and search on Keycloak users.
/// </summary>
public interface IKeycloakUserService
{
    /// <summary>
    /// Creates a new user in Keycloak.
    /// </summary>
    Task<string?> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        bool emailVerified = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user in Keycloak with additional attributes.
    /// </summary>
    Task<string?> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        string? password = null,
        bool emailVerified = false,
        Dictionary<string, List<string>>? attributes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in Keycloak.
    /// </summary>
    Task UpdateUserAsync(
        string keycloakUserId,
        string? firstName = null,
        string? lastName = null,
        bool? emailVerified = null,
        bool? enabled = null,
        Dictionary<string, List<string>>? attributes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user from Keycloak.
    /// </summary>
    Task DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their Keycloak ID.
    /// </summary>
    Task<KeycloakUserDto?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<KeycloakUserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a verification email to the user.
    /// </summary>
    Task SendVerificationEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password in Keycloak.
    /// </summary>
    Task<bool> ResetPasswordAsync(string keycloakUserId, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables a user.
    /// </summary>
    Task SetUserEnabledAsync(string keycloakUserId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by invalidating their session.
    /// </summary>
    Task LogoutAsync(string keycloakUserId, CancellationToken cancellationToken = default);
}
