using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public AuthenticateCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IIdentityEventPublisher eventPublisher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.Verify(request.Password, user.Password))
            return Result.Fail<UserDto>(new UnauthorizedError("Invalid Credentials"));

        if (!user.Status)
            return Result.Fail<UserDto>(new ForbiddenError("User Not Verified"));

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Token = _jwtTokenService.GenerateToken(user.Id.ToString());

        await _eventPublisher.PublishUserLoggedInAsync(user);

        return Result.Ok(userDto);
    }
}
