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
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="password">The user's password (optional, will require password reset if not provided).</param>
    /// <param name="emailVerified">Whether the email is verified.</param>
    /// <param name="attributes">Additional user attributes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Keycloak user ID.</returns>
    Task<string> CreateUserAsync(
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
    Task SendPasswordResetEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default);

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

/// <summary>
/// Represents a user from Keycloak.
/// </summary>
public class KeycloakUserDto
{
    public string Id { get; set; } = null!;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool EmailVerified { get; set; }
    public bool Enabled { get; set; }
    public Dictionary<string, List<string>>? Attributes { get; set; }
}

/// <summary>
/// Represents token response from Keycloak.
/// </summary>
public class KeycloakTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string? IdToken { get; set; }
}
