namespace CryptoJackpot.Identity.Application.Requests;

public class CreateUserRequest
{
    public long? Id { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public long RoleId { get; set; }
    public long CountryId { get; set; }
    public string StatePlace { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Address { get; set; }
    public bool Status { get; set; }
    public string? ImagePath { get; set; }
    public string? GoogleAccessToken { get; set; }
    public string? GoogleRefreshToken { get; set; }
    public string? ReferralCode { get; set; }
}

