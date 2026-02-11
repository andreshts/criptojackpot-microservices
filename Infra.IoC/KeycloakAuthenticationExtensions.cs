using System.Security.Claims;
using CryptoJackpot.Infra.IoC.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CryptoJackpot.Infra.IoC;

/// <summary>
/// Extension methods for configuring Keycloak OIDC authentication in microservices.
/// </summary>
public static class KeycloakAuthenticationExtensions
{
    /// <summary>
    /// Claim type for roles in Keycloak tokens.
    /// </summary>
    private const string RolesClaimType = "roles";
    
    /// <summary>
    /// Adds Keycloak JWT Bearer authentication to the service collection.
    /// All microservices should use this method for consistent token validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakSettings = configuration.GetSection("Keycloak").Get<KeycloakSettings>()
            ?? throw new InvalidOperationException("Keycloak settings are not configured");

        services.Configure<KeycloakSettings>(configuration.GetSection("Keycloak"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Set the authority to the Keycloak realm URL
            options.Authority = keycloakSettings.GetRealmUrl();
            
            // Audience validation
            options.Audience = keycloakSettings.Audience ?? keycloakSettings.ClientId;
            
            // For development, we may need to disable HTTPS requirement
            options.RequireHttpsMetadata = keycloakSettings.RequireHttpsMetadata;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Issuer is auto-discovered from the OIDC metadata at {Authority}/.well-known/openid-configuration.
                // This allows the Authority URL (e.g., internal K8s service http://keycloak:8080) to differ
                // from the token issuer (e.g., external hostname http://auth.cryptojackpot.local:30180).
                ValidateIssuer = true,

                ValidateAudience = keycloakSettings.ValidateAudience,
                ValidAudience = keycloakSettings.Audience ?? keycloakSettings.ClientId,

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                // Keycloak uses RS256 by default, keys are fetched from JWKS endpoint
                // The middleware automatically fetches keys from {authority}/.well-known/openid-configuration

                // Map the 'sub' claim to ClaimTypes.NameIdentifier
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = RolesClaimType,

                // Clock skew tolerance (default is 5 minutes)
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    // Log authentication failures for debugging
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Extract user_id from custom claim and add as NameIdentifier if present
                    var userId = context.Principal?.FindFirst("user_id")?.Value;
                    if (!string.IsNullOrEmpty(userId) && context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        // Add the user_id as the primary identifier for the application
                        identity.AddClaim(new Claim("app_user_id", userId));
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            // Add role-based policies
            options.AddPolicy("RequireAdminRole", policy => 
                policy.RequireClaim(RolesClaimType, "admin"));
            
            options.AddPolicy("RequireModeratorRole", policy => 
                policy.RequireClaim(RolesClaimType, "admin", "moderator"));
            
            options.AddPolicy("RequireUserRole", policy => 
                policy.RequireClaim(RolesClaimType, "admin", "moderator", "user"));
        });

        return services;
    }

    /// <summary>
    /// Gets the application user ID from the ClaimsPrincipal.
    /// This retrieves the user_id custom attribute set in Keycloak.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user ID or null if not found.</returns>
    public static long? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("user_id")?.Value 
            ?? principal.FindFirst("app_user_id")?.Value;
        
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the Keycloak subject (sub) claim from the ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The Keycloak subject ID.</returns>
    public static string? GetKeycloakSubject(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the user's email from the ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The email address.</returns>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role.</returns>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.HasClaim(RolesClaimType, role);
    }
}
