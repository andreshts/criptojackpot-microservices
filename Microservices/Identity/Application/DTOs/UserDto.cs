namespace CryptoJackpot.Identity.Application.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public Guid UserGuid { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public string? Phone { get; set; }
    public long CountryId { get; set; }
    public string StatePlace { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Address { get; set; }
    public string? ImagePath { get; set; }
    public bool Status { get; set; }
    public string? Token { get; set; }
    public RoleDto? Role { get; set; }
}
