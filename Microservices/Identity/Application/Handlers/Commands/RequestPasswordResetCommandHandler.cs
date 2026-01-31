using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles password reset requests by triggering Keycloak's password reset email.
/// </summary>
public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result<string>>
{
    private const string PasswordResetSuccessMessage = "If the email exists, a password reset link has been sent";
    
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
        {
            // Don't reveal if user exists - security best practice
            _logger.LogWarning("Password reset requested for non-existent email {Email}", request.Email);
            return Result.Ok(PasswordResetSuccessMessage);
        }

        if (string.IsNullOrEmpty(user.KeycloakId))
        {
            _logger.LogWarning("Password reset requested for user without KeycloakId: {Email}", request.Email);
            return Result.Ok(PasswordResetSuccessMessage);
        }

        try
        {
            // Trigger Keycloak to send password reset email
            var success = await _keycloakAdminService.SendPasswordResetEmailAsync(user.KeycloakId, cancellationToken);
            
            if (!success)
            {
                _logger.LogError("Failed to send password reset email via Keycloak for {Email}", request.Email);
                // Still return success message to avoid user enumeration
            }

            _logger.LogInformation("Password reset requested for {Email}", request.Email);
            return Result.Ok(PasswordResetSuccessMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process password reset for {Email}", request.Email);
            // Still return success message to avoid user enumeration
            return Result.Ok(PasswordResetSuccessMessage);
        }
    }
}
