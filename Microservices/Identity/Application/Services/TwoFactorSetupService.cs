using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Service for managing 2FA setup, confirmation, and disabling.
/// Handles encryption of secrets and generation of recovery codes.
/// </summary>
public class TwoFactorSetupService : ITwoFactorSetupService
{
    private readonly IUserRepository _userRepository;
    private readonly IRecoveryCodeRepository _recoveryCodeRepository;
    private readonly ITotpService _totpService;
    private readonly IRecoveryCodeService _recoveryCodeService;
    private readonly IDataEncryptionService _encryptionService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TwoFactorConfig _config;
    private readonly ILogger<TwoFactorSetupService> _logger;

    public TwoFactorSetupService(
        IUserRepository userRepository,
        IRecoveryCodeRepository recoveryCodeRepository,
        ITotpService totpService,
        IRecoveryCodeService recoveryCodeService,
        IDataEncryptionService encryptionService,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IOptions<TwoFactorConfig> config,
        ILogger<TwoFactorSetupService> logger)
    {
        _userRepository = userRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
        _totpService = totpService;
        _recoveryCodeService = recoveryCodeService;
        _encryptionService = encryptionService;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<Setup2FaResultDto>> InitiateSetupAsync(
        Guid userGuid, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(userGuid);
        if (user is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        if (user.TwoFactorEnabled)
        {
            return Result.Fail(new ConflictError("2FA is already enabled. Disable it first to set up again."));
        }

        // Generate new secret
        var plainSecret = _totpService.GenerateSecret();
        
        // Encrypt and store
        user.TwoFactorSecret = _encryptionService.Encrypt(plainSecret);
        user.TwoFactorEnabled = false; // Explicitly keep disabled until confirmed
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("2FA setup initiated for user {UserId}", user.Id);

        // Return plain secret for QR code
        return Result.Ok(new Setup2FaResultDto
        {
            Secret = plainSecret,
            QrCodeUri = _totpService.GenerateQrCodeUri(user.Email, plainSecret, _config.Issuer)
        });
    }

    public async Task<Result<Confirm2FaResultDto>> ConfirmSetupAsync(
        Guid userGuid, 
        string code, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(userGuid);
        if (user is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        if (user.TwoFactorEnabled)
        {
            return Result.Fail(new ConflictError("2FA is already enabled."));
        }

        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            return Result.Fail(new BadRequestError("2FA setup not initiated. Call setup endpoint first."));
        }

        // Decrypt secret and validate code
        var decryptedSecret = _encryptionService.Decrypt(user.TwoFactorSecret);
        if (string.IsNullOrWhiteSpace(decryptedSecret))
        {
            _logger.LogError("Failed to decrypt TwoFactorSecret during confirmation for user {UserId}", user.Id);
            return Result.Fail(new UnauthorizedError("2FA configuration error. Please start setup again."));
        }

        if (!_totpService.ValidateCode(decryptedSecret, code))
        {
            _logger.LogWarning("Invalid TOTP code during 2FA confirmation for user {UserId}", user.Id);
            return Result.Fail(new UnauthorizedError("Invalid verification code. Please try again."));
        }

        // Enable 2FA
        user.TwoFactorEnabled = true;

        // Generate recovery codes
        var (plainCodes, codeEntities) = _recoveryCodeService.GenerateCodes(user.Id, _config.RecoveryCodeCount);
        
        // Delete any existing codes (shouldn't exist, but just in case)
        await _recoveryCodeRepository.DeleteAllByUserIdAsync(user.Id);
        await _recoveryCodeRepository.AddRangeAsync(codeEntities);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("2FA enabled for user {UserId}. Generated {CodeCount} recovery codes.", 
            user.Id, plainCodes.Count);

        // Publish security event
        await _eventPublisher.PublishSecurityAlertAsync(
            user,
            SecurityAlertType.TwoFactorEnabled,
            "Two-factor authentication has been enabled on your account.",
            null,
            null);

        return Result.Ok(new Confirm2FaResultDto
        {
            RecoveryCodes = plainCodes
        });
    }

    public async Task<Result> DisableAsync(
        Guid userGuid, 
        string? code, 
        string? recoveryCode, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidWithRecoveryCodesAsync(userGuid);
        if (user is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        if (!user.TwoFactorEnabled)
        {
            return Result.Fail(new BadRequestError("2FA is not enabled."));
        }

        // Verify with TOTP code
        if (!string.IsNullOrWhiteSpace(code))
        {
            var decryptedSecret = _encryptionService.Decrypt(user.TwoFactorSecret!);
            if (string.IsNullOrWhiteSpace(decryptedSecret) || !_totpService.ValidateCode(decryptedSecret, code))
            {
                return Result.Fail(new UnauthorizedError("Invalid verification code."));
            }
        }
        // Or verify with recovery code
        else if (!string.IsNullOrWhiteSpace(recoveryCode))
        {
            var matchingCode = _recoveryCodeService.ValidateCode(recoveryCode, user.RecoveryCodes);
            if (matchingCode is null)
            {
                return Result.Fail(new UnauthorizedError("Invalid recovery code."));
            }
            // Mark as used (even though we're deleting all codes)
            matchingCode.MarkAsUsed();
        }
        else
        {
            return Result.Fail(new BadRequestError("Verification code or recovery code is required."));
        }

        // Disable 2FA
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;

        // Delete all recovery codes
        await _recoveryCodeRepository.DeleteAllByUserIdAsync(user.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("2FA disabled for user {UserId}", user.Id);

        // Publish security event
        await _eventPublisher.PublishSecurityAlertAsync(
            user,
            SecurityAlertType.TwoFactorDisabled,
            "Two-factor authentication has been disabled on your account.",
            null,
            null);

        return Result.Ok();
    }
}

