using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Identity.Domain.Models;

public class User : BaseEntity
{
    // ─── Identidad ───────────────────────────────────────────────

    /// <summary>
    /// GUID externo para exposición en APIs y comunicación entre microservicios.
    /// Nunca exponer el Id (long) fuera del Identity bounded context.
    /// </summary>
    public Guid UserGuid { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Token de un solo uso para verificación de email.
    /// Se genera al registrar o al solicitar re-verificación.
    /// </summary>
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    // ─── Credenciales ────────────────────────────────────────────

    /// <summary>
    /// Hash BCrypt del password. Null cuando el usuario se registra 
    /// exclusivamente via Google OAuth (no tiene password local).
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Token y expiración para flujo de password reset.
    /// </summary>
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    // ─── Login Retry / Account Lockout ───────────────────────────

    /// <summary>
    /// Contador de intentos fallidos consecutivos de login.
    /// Se resetea a 0 tras login exitoso.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Fecha/hora hasta la cual la cuenta está bloqueada.
    /// Null = cuenta no bloqueada. Se aplica lockout progresivo:
    ///   3 intentos → 1 min | 5 intentos → 5 min | 7+ intentos → 30 min
    /// </summary>
    public DateTime? LockoutEndAt { get; set; }

    /// <summary>
    /// Timestamp del último login exitoso (local o Google).
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // ─── Google OAuth ────────────────────────────────────────────

    /// <summary>
    /// Subject ID del usuario en Google ('sub' claim del ID token).
    /// Permite vincular la cuenta local con Google sin depender de email matching.
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// Access token de Google para llamadas a APIs de Google (Calendar, Drive, etc.).
    /// Encriptado en reposo via IDataProtector.
    /// </summary>
    public string? GoogleAccessToken { get; set; }

    /// <summary>
    /// Refresh token de Google para renovar el access token sin re-autenticación.
    /// Encriptado en reposo via IDataProtector. Solo se recibe en el primer consent.
    /// </summary>
    public string? GoogleRefreshToken { get; set; }

    /// <summary>
    /// Expiración del Google access token actual.
    /// </summary>
    public DateTime? GoogleTokenExpiresAt { get; set; }

    // ─── Two-Factor Authentication (TOTP) ────────────────────────

    /// <summary>
    /// Indica si el usuario ha completado la configuración de 2FA.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Shared secret (Base32) para generación de códigos TOTP.
    /// Encriptado en reposo via IDataProtector.
    /// Se genera al iniciar setup de 2FA; se confirma cuando el usuario 
    /// valida el primer código correctamente.
    /// </summary>
    public string? TwoFactorSecret { get; set; }

    /// <summary>
    /// Códigos de recuperación de un solo uso (hashed individualmente).
    /// Permiten acceso si el usuario pierde su dispositivo TOTP.
    /// Se generan al activar 2FA (8 códigos) y se consumen al usarse.
    /// </summary>
    public ICollection<UserRecoveryCode> RecoveryCodes { get; set; } = new List<UserRecoveryCode>();

    // ─── Perfil ──────────────────────────────────────────────────

    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public long CountryId { get; set; }
    public string StatePlace { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Address { get; set; }
    public bool Status { get; set; }
    public string? ImagePath { get; set; }

    // ─── Seguridad – Refresh Tokens ──────────────────────────────

    /// <summary>
    /// Colección de refresh tokens activos (uno por dispositivo/sesión).
    /// Permite revocación selectiva y detección de token reuse.
    /// </summary>
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();

    // ─── Relaciones ──────────────────────────────────────────────

    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Country Country { get; set; } = null!;
    public ICollection<UserReferral> Referrals { get; set; } = new List<UserReferral>();
    public UserReferral? ReferredBy { get; set; }

    // ─── Domain Methods ──────────────────────────────────────────

    /// <summary>
    /// Registra un intento de login fallido y aplica lockout progresivo.
    /// </summary>
    public void RegisterFailedLogin()
    {
        FailedLoginAttempts++;
        LockoutEndAt = FailedLoginAttempts switch
        {
            >= 7 => DateTime.UtcNow.AddMinutes(30),
            >= 5 => DateTime.UtcNow.AddMinutes(5),
            >= 3 => DateTime.UtcNow.AddMinutes(1),
            _ => null
        };
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resetea el contador de intentos fallidos tras login exitoso.
    /// </summary>
    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si la cuenta está actualmente bloqueada.
    /// </summary>
    public bool IsLockedOut => LockoutEndAt.HasValue && LockoutEndAt.Value > DateTime.UtcNow;

    /// <summary>
    /// Indica si el usuario se registró exclusivamente via Google (sin password local).
    /// </summary>
    public bool IsExternalOnly => PasswordHash is null && GoogleId is not null;
}