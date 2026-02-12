using CryptoJackpot.Domain.Core.Models;
namespace CryptoJackpot.Identity.Domain.Models;

public class UserRecoveryCode : BaseEntity
{
    public long UserId { get; set; }

    /// <summary>
    /// Hash SHA-256 del código de recuperación.
    /// El código original (formato: XXXX-XXXX) solo se muestra una vez al usuario.
    /// </summary>
    public string CodeHash { get; set; } = null!;

    /// <summary>
    /// Indica si el código ya fue consumido.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Fecha en que fue consumido. Null si aún está disponible.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    // ─── Navegación ──────────────────────────────────────────────
    
    public User User { get; set; } = null!;

    // ─── Domain Methods ──────────────────────────────────────────
    
    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}