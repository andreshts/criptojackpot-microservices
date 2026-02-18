namespace CryptoJackpot.Identity.Application.DTOs;

public class UserDto
{
    public Guid UserGuid { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public string? Phone { get; set; }
    public string? ImagePath { get; set; }
    public bool Status { get; set; }
    public string? Token { get; set; }
    public RoleDto? Role { get; set; }
}
