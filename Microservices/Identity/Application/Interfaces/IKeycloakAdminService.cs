using CryptoJackpot.Identity.Application.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Interface for managing users in Keycloak via the Admin REST API.
/// </summary>
public interface IKeycloakAdminService
{
    /// <summary>
    /// Creates a new user in Keycloak.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="emailVerified">Whether the email is verified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Keycloak user ID, or null if creation failed.</returns>
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
    /// Assigns a realm role to a user.
    /// </summary>
    Task AssignRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a realm role from a user.
    /// </summary>
    Task RemoveRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a verification email to the user.
    /// </summary>
    Task SendVerificationEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    /// <returns>True if the email was sent successfully.</returns>
    Task<bool> SendPasswordResetEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password in Keycloak.
    /// </summary>
    /// <returns>True if the password was reset successfully.</returns>
    Task<bool> ResetPasswordAsync(string keycloakUserId, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables a user.
    /// </summary>
    Task SetUserEnabledAsync(string keycloakUserId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges credentials for tokens using the Resource Owner Password Credentials flow.
    /// Used for legacy login endpoint compatibility.
    /// </summary>
    Task<KeycloakTokenResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    Task<KeycloakTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by invalidating their session.
    /// </summary>
    Task LogoutAsync(string keycloakUserId, CancellationToken cancellationToken = default);
}

