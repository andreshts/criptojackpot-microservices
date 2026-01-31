using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public AuthenticateCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService,
        IIdentityEventPublisher eventPublisher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        // Authenticate with Keycloak first
        var tokenResponse = await _keycloakAdminService.GetTokenAsync(request.Email, request.Password, cancellationToken);
        
        if (tokenResponse == null)
            return Result.Fail<AuthResponseDto>(new UnauthorizedError("Invalid Credentials"));

        // Get user from local database for additional info
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            return Result.Fail<AuthResponseDto>(new UnauthorizedError("User not found"));

        if (!user.Status)
            return Result.Fail<AuthResponseDto>(new ForbiddenError("User Not Verified"));

        var authResponse = new AuthResponseDto
        {
            UserGuid = user.UserGuid,
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            ImagePath = user.ImagePath,
            Status = user.Status,
            Role = user.Role != null ? _mapper.Map<RoleDto>(user.Role) : null,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            RefreshExpiresIn = tokenResponse.RefreshExpiresIn,
            TokenType = tokenResponse.TokenType
        };

        await _eventPublisher.PublishUserLoggedInAsync(user);

        return Result.Ok(authResponse);
    }
}
