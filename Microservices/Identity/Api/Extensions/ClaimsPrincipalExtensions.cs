using System.Security.Claims;

namespace CryptoJackpot.Identity.Api.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to extract user information.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the UserGuid from JWT claims (sub or NameIdentifier).
    /// </summary>
    /// <param name="principal">The claims principal from User property</param>
    /// <returns>UserGuid if found and valid, null otherwise</returns>
    public static Guid? GetUserGuid(this ClaimsPrincipal principal)
    {
        var userGuidClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? principal.FindFirst("sub")?.Value;
        
        return Guid.TryParse(userGuidClaim, out var guid) ? guid : null;
    }

    /// <summary>
    /// Gets the UserGuid from JWT claims, throwing if not found.
    /// Use this in [Authorize] endpoints where user must be authenticated.
    /// </summary>
    /// <param name="principal">The claims principal from User property</param>
    /// <returns>UserGuid</returns>
    /// <exception cref="UnauthorizedAccessException">If UserGuid claim is not found</exception>
    public static Guid GetRequiredUserGuid(this ClaimsPrincipal principal)
    {
        return principal.GetUserGuid() 
               ?? throw new UnauthorizedAccessException("User identifier not found in token.");
    }

    /// <summary>
    /// Gets the user's email from JWT claims.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value 
               ?? principal.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Gets the user's role from JWT claims.
    /// </summary>
    public static string? GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value 
               ?? principal.FindFirst("role")?.Value;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role) || principal.GetRole() == role;
    }
}

