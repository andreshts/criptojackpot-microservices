using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for managing 2FA setup and configuration.
/// </summary>
public interface ITwoFactorSetupService
{
    /// <summary>
    /// Initiates 2FA setup by generating a new secret.
    /// Secret is stored encrypted but 2FA remains disabled until confirmed.
    /// </summary>
    Task<Result<Setup2FaResultDto>> InitiateSetupAsync(Guid userGuid, CancellationToken cancellationToken);

    /// <summary>
    /// Confirms 2FA setup by validating TOTP code.
    /// Enables 2FA and generates recovery codes.
    /// </summary>
    Task<Result<Confirm2FaResultDto>> ConfirmSetupAsync(Guid userGuid, string code, CancellationToken cancellationToken);

    /// <summary>
    /// Disables 2FA after validating TOTP or recovery code.
    /// Removes secret and all recovery codes.
    /// </summary>
    Task<Result> DisableAsync(Guid userGuid, string? code, string? recoveryCode, CancellationToken cancellationToken);
}

