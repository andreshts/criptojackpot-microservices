using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles password reset. With Keycloak integration, this is typically handled
/// directly by Keycloak's forgot password flow. This handler is kept for 
/// backward compatibility or custom flows.
/// </summary>
public class ResetPasswordWithCodeCommandHandler : IRequestHandler<ResetPasswordWithCodeCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly IMapper _mapper;

    public ResetPasswordWithCodeCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(ResetPasswordWithCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        if (string.IsNullOrEmpty(user.KeycloakId))
            return Result.Fail<UserDto>(new BadRequestError("User not linked to authentication service"));

        if (request.NewPassword != request.ConfirmPassword)
            return Result.Fail<UserDto>(new BadRequestError("Passwords do not match"));

        // Reset password in Keycloak
        var success = await _keycloakAdminService.ResetPasswordAsync(user.KeycloakId, request.NewPassword, cancellationToken);
        if (!success)
            return Result.Fail<UserDto>(new InternalServerError("Failed to reset password"));

        return Result.Ok(_mapper.Map<UserDto>(user));
    }
}

