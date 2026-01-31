using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles email confirmation using the verification token sent via Notification Service.
/// Also syncs the verified status with Keycloak.
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;

    public ConfirmEmailCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
    }

    public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Fail<string>(new BadRequestError("Invalid confirmation token"));

        // Find user by verification token
        var user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token);

        if (user == null)
            return Result.Fail<string>(new NotFoundError("Invalid or expired confirmation token"));

        if (user.Status)
            return Result.Fail<string>(new BadRequestError("Email already confirmed"));

        // Validate and consume the token
        if (!user.ValidateAndConsumeEmailVerificationToken(request.Token))
            return Result.Fail<string>(new BadRequestError("Invalid or expired confirmation token"));

        // Update Keycloak to mark email as verified
        if (!string.IsNullOrEmpty(user.KeycloakId))
        {
            await _keycloakAdminService.UpdateUserAsync(
                user.KeycloakId,
                emailVerified: true,
                cancellationToken: cancellationToken);
        }

        // Save changes to local database
        await _userRepository.UpdateAsync(user);

        return Result.Ok("Email confirmed successfully");
    }
}
