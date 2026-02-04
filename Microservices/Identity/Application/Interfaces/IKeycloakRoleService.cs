namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Interface for managing realm and client role mappings in Keycloak.
/// </summary>
public interface IKeycloakRoleService
{
    /// <summary>
    /// Assigns a realm role to a user.
    /// </summary>
    Task AssignRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a realm role from a user.
    /// </summary>
    Task RemoveRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all realm roles assigned to a user.
    /// </summary>
    Task<IReadOnlyList<string>> GetUserRolesAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    Task<bool> HasRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default);
}
