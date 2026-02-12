using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Service for Google OAuth login flow.
/// Handles three scenarios: existing user, linking accounts, and registration.
/// </summary>
public class GoogleLoginService : IGoogleLoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDataEncryptionService _encryptionService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GoogleLoginService> _logger;

    public GoogleLoginService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDataEncryptionService encryptionService,
        IAuthenticationService authenticationService,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<GoogleLoginService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _encryptionService = encryptionService;
        _authenticationService = authenticationService;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResultDto>> LoginOrRegisterAsync(
        GoogleUserPayload payload,
        GoogleLoginContext context,
        CancellationToken cancellationToken)
    {
        // Step 1: Try to find user by GoogleId (primary link)
        var user = await _userRepository.GetByGoogleIdAsync(payload.GoogleId);

        if (user is not null)
        {
            return await HandleExistingGoogleUserAsync(user, context, cancellationToken);
        }

        // Step 2: Try to find user by email (for account linking)
        user = await _userRepository.GetByEmailAsync(payload.Email);

        if (user is not null)
        {
            return await HandleAccountLinkingAsync(user, payload, context, cancellationToken);
        }

        // Step 3: Create new user
        return await HandleNewUserRegistrationAsync(payload, context, cancellationToken);
    }

    private async Task<Result<LoginResultDto>> HandleExistingGoogleUserAsync(
        User user,
        GoogleLoginContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Google login for existing user {UserId}", user.Id);

        if (user.IsLockedOut)
        {
            return CreateLockedOutError(user);
        }

        UpdateGoogleTokens(user, context);

        if (user.TwoFactorEnabled)
        {
            return await _authenticationService.HandleTwoFactorLoginAsync(user, cancellationToken);
        }

        return await _authenticationService.CompleteLoginAsync(
            user, context.DeviceInfo, context.IpAddress, context.RememberMe, cancellationToken);
    }

    private async Task<Result<LoginResultDto>> HandleAccountLinkingAsync(
        User user,
        GoogleUserPayload payload,
        GoogleLoginContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Linking Google account to existing user {UserId}", user.Id);

        if (user.IsLockedOut)
        {
            return CreateLockedOutError(user);
        }

        // Link Google account
        user.GoogleId = payload.GoogleId;
        UpdateGoogleTokens(user, context);

        // Auto-verify email if Google confirms it
        if (!user.EmailVerified && payload.EmailVerified)
        {
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishSecurityAlertAsync(
            user,
            SecurityAlertType.NewDeviceLogin,
            "Google account linked to your profile.",
            context.IpAddress,
            context.DeviceInfo);

        if (user.TwoFactorEnabled)
        {
            return await _authenticationService.HandleTwoFactorLoginAsync(user, cancellationToken);
        }

        return await _authenticationService.CompleteLoginAsync(
            user, context.DeviceInfo, context.IpAddress, context.RememberMe, cancellationToken);
    }

    private async Task<Result<LoginResultDto>> HandleNewUserRegistrationAsync(
        GoogleUserPayload payload,
        GoogleLoginContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user via Google: {Email}", payload.Email);

        var defaultRole = await _roleRepository.GetDefaultRoleAsync();
        if (defaultRole is null)
        {
            _logger.LogError("Default role 'User' not found in database");
            return Result.Fail(new InternalServerError("Configuration error. Please contact support."));
        }

        var user = CreateUserFromGooglePayload(payload, defaultRole.Id);
        UpdateGoogleTokens(user, context);

        await _userRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered via Google: {UserId}", user.Id);

        await _eventPublisher.PublishExternalUserRegisteredAsync(user);

        return await _authenticationService.CompleteLoginAsync(
            user, context.DeviceInfo, context.IpAddress, context.RememberMe, cancellationToken);
    }

    private Result<LoginResultDto> CreateLockedOutError(User user)
    {
        var lockoutMinutes = _authenticationService.GetLockoutMinutes(user.FailedLoginAttempts);
        return Result.Fail(new LockedError(
            $"Account is locked. Try again in {lockoutMinutes} minutes.",
            lockoutMinutes * 60));
    }

    private static User CreateUserFromGooglePayload(GoogleUserPayload payload, long roleId)
    {
        return new User
        {
            Email = payload.Email,
            EmailVerified = payload.EmailVerified,
            Name = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "User",
            LastName = payload.FamilyName ?? payload.Name?.Split(' ').LastOrDefault() ?? "",
            GoogleId = payload.GoogleId,
            ImagePath = payload.Picture,
            RoleId = roleId,
            CountryId = 1, // Default country - should be configurable
            StatePlace = "Not specified",
            City = "Not specified",
            Status = true,
            PasswordHash = null // Google-only user
        };
    }

    private void UpdateGoogleTokens(User user, GoogleLoginContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.AccessToken))
        {
            user.GoogleAccessToken = _encryptionService.Encrypt(context.AccessToken);
            user.GoogleTokenExpiresAt = context.ExpiresIn.HasValue 
                ? DateTime.UtcNow.AddSeconds(context.ExpiresIn.Value) 
                : null;
        }

        // Refresh token only provided on first consent - don't overwrite if null
        if (!string.IsNullOrWhiteSpace(context.RefreshToken))
        {
            user.GoogleRefreshToken = _encryptionService.Encrypt(context.RefreshToken);
        }
    }
}
