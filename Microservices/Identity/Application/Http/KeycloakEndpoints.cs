namespace CryptoJackpot.Identity.Application.Http;

/// <summary>
/// Constants for Keycloak Admin REST API endpoints.
/// Centralizes all URL patterns to avoid hardcoded strings and typos.
/// </summary>
public static class KeycloakEndpoints
{
    /// <summary>
    /// OpenID Connect protocol endpoints
    /// </summary>
    public static class Oidc
    {
        public const string Token = "/protocol/openid-connect/token";
        public const string UserInfo = "/protocol/openid-connect/userinfo";
        public const string Logout = "/protocol/openid-connect/logout";
        public const string WellKnown = "/.well-known/openid-configuration";
    }

    /// <summary>
    /// Admin REST API user endpoints
    /// </summary>
    public static class Users
    {
        public const string Base = "/users";
        
        public static string ById(string userId) => $"/users/{userId}";
        public static string ByEmail(string email) => $"/users?email={Uri.EscapeDataString(email)}&exact=true";
        public static string ByUsername(string username) => $"/users?username={Uri.EscapeDataString(username)}&exact=true";
        public static string Search(string search, int? first = null, int? max = null)
        {
            var query = $"/users?search={Uri.EscapeDataString(search)}";
            if (first.HasValue) query += $"&first={first.Value}";
            if (max.HasValue) query += $"&max={max.Value}";
            return query;
        }

        public static string Logout(string userId) => $"/users/{userId}/logout";
        public static string SendVerifyEmail(string userId) => $"/users/{userId}/send-verify-email";
        public static string ExecuteActionsEmail(string userId) => $"/users/{userId}/execute-actions-email";
        public static string ResetPassword(string userId) => $"/users/{userId}/reset-password";
        public static string RoleMappingsRealm(string userId) => $"/users/{userId}/role-mappings/realm";
    }

    /// <summary>
    /// Admin REST API role endpoints
    /// </summary>
    public static class Roles
    {
        public const string Base = "/roles";
        
        public static string ByName(string roleName) => $"/roles/{roleName}";
        public static string Composites(string roleName) => $"/roles/{roleName}/composites";
    }

    /// <summary>
    /// Admin REST API client endpoints
    /// </summary>
    public static class Clients
    {
        public const string Base = "/clients";
        
        public static string ById(string clientId) => $"/clients/{clientId}";
        public static string Roles(string clientId) => $"/clients/{clientId}/roles";
        public static string RoleByName(string clientId, string roleName) => $"/clients/{clientId}/roles/{roleName}";
    }

    /// <summary>
    /// Admin REST API group endpoints
    /// </summary>
    public static class Groups
    {
        public const string Base = "/groups";
        
        public static string ById(string groupId) => $"/groups/{groupId}";
        public static string UserGroups(string userId) => $"/users/{userId}/groups";
    }
}
