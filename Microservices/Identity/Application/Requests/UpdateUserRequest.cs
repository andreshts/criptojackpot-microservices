using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Identity.Application.Requests;

public class UpdateUserRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    [Required]
    public long CountryId { get; set; }

    [Required]
    public string StatePlace { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    public string? Address { get; set; }
}
