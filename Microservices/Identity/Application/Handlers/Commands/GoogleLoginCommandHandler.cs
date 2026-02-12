using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles Google OAuth login/registration.
/// </summary>
public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<LoginResultDto>>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IGoogleLoginService _googleLoginService;

    public GoogleLoginCommandHandler(
        IGoogleAuthService googleAuthService,
        IGoogleLoginService googleLoginService)
    {
        _googleAuthService = googleAuthService;
        _googleLoginService = googleLoginService;
    }

    public async Task<Result<LoginResultDto>> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Validate Google ID token
        var payload = await _googleAuthService.ValidateIdTokenAsync(request.IdToken);
        if (payload is null)
        {
            return Result.Fail(new UnauthorizedError("Invalid Google ID token."));
        }

        // Step 2: Create context and login/register user
        var context = new GoogleLoginContext
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken,
            ExpiresIn = request.ExpiresIn,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            RememberMe = request.RememberMe
        };

        return await _googleLoginService.LoginOrRegisterAsync(payload, context, cancellationToken);
    }
}

