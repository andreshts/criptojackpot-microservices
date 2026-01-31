namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Response DTO for authentication that includes Keycloak tokens.
/// </summary>
public class AuthResponseDto
{
    public Guid UserGuid { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ImagePath { get; set; }
    public bool Status { get; set; }
    public RoleDto? Role { get; set; }
    
    // Keycloak tokens
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
