using CryptoJackpot.Domain.Core.Models;
namespace CryptoJackpot.Identity.Domain.Models;

public class UserRefreshToken : BaseEntity
{
    public long UserId { get; set; }

    /// <summary>
    /// Hash SHA-256 del refresh token. Nunca almacenar el token en texto plano.
    /// El BFF recibe el token raw via HttpOnly cookie; aquí solo se guarda el hash.
    /// </summary>
    public string TokenHash { get; set; } = null!;

    /// <summary>
    /// Identificador único de la "familia" de tokens para refresh token rotation.
    /// Todos los tokens derivados de un mismo login comparten el mismo FamilyId.
    /// Permite revocar toda la cadena si se detecta reuso.
    /// </summary>
    public Guid FamilyId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Fecha de expiración absoluta del refresh token.
    /// Típicamente 7 días para sesiones normales, 30 días con "remember me".
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indica si el token fue revocado explícitamente (logout) o por rotación.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Fecha en que fue revocado. Null si aún está activo.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Razón de revocación para auditoría.
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Token que reemplazó a este (tras rotación). 
    /// Permite trazar la cadena completa de refresh tokens.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>
    /// Identificador del dispositivo/sesión (fingerprint, User-Agent hash, etc.).
    /// Permite mostrar al usuario sus sesiones activas.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP desde la cual se emitió el token.
    /// </summary>
    public string? IpAddress { get; set; }

    // ─── Navegación ──────────────────────────────────────────────

    public User User { get; set; } = null!;

    // ─── Domain Methods ──────────────────────────────────────────

    private bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Revoca este token con una razón específica.
    /// </summary>
    public void Revoke(string reason, string? replacedByTokenHash = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
        UpdatedAt = DateTime.UtcNow;
    }
}