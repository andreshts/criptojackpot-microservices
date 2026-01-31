using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly IMapper _mapper;

    public UpdatePasswordCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        if (string.IsNullOrEmpty(user.KeycloakId))
            return Result.Fail<UserDto>(new BadRequestError("User not linked to authentication service"));

        // Verify current password by attempting to get a token
        var tokenResult = await _keycloakAdminService.GetTokenAsync(user.Email, request.CurrentPassword, cancellationToken);
        if (tokenResult is null)
            return Result.Fail<UserDto>(new BadRequestError("Invalid current password"));

        // Update password in Keycloak
        var success = await _keycloakAdminService.ResetPasswordAsync(user.KeycloakId, request.NewPassword, cancellationToken);
        if (!success)
            return Result.Fail<UserDto>(new InternalServerError("Failed to update password"));

        return Result.Ok(_mapper.Map<UserDto>(user));
    }
}

