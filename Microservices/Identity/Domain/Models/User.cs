using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Identity.Domain.Models;

public class User : BaseEntity
{
    /// <summary>
    /// External GUID for API exposure and cross-service communication
    /// </summary>
    public Guid UserGuid { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Keycloak user ID for authentication integration
    /// </summary>
    public string? KeycloakId { get; set; }
    
    /// <summary>
    /// Token for email verification (sent via Notification Service)
    /// </summary>
    public string? EmailVerificationToken { get; set; }
    
    /// <summary>
    /// Expiration time for the email verification token
    /// </summary>
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public long CountryId { get; set; }
    public string StatePlace { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Address { get; set; }
    public bool Status { get; set; }
    public string? ImagePath { get; set; }
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Country Country { get; set; } = null!;

    // Navegación: Usuarios que este usuario ha referido
    public ICollection<UserReferral> Referrals { get; set; } = new List<UserReferral>();

    // Navegación: Referido por 
    public UserReferral? ReferredBy { get; set; }
    
    /// <summary>
    /// Generates a new email verification token valid for 24 hours
    /// </summary>
    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = Guid.NewGuid().ToString("N");
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
    }
    
    /// <summary>
    /// Validates and consumes the email verification token
    /// </summary>
    public bool ValidateAndConsumeEmailVerificationToken(string token)
    {
        if (string.IsNullOrEmpty(EmailVerificationToken) ||
            EmailVerificationToken != token ||
            EmailVerificationTokenExpiry == null ||
            EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }
        
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
        Status = true;
        return true;
    }
}