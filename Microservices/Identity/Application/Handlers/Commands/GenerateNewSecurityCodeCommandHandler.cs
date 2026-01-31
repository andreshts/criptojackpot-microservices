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
/// Regenerates the email verification token and resends the verification email
/// via the Notification Service.
/// </summary>
public class GenerateNewSecurityCodeCommandHandler : IRequestHandler<GenerateNewSecurityCodeCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public GenerateNewSecurityCodeCommandHandler(
        IUserRepository userRepository,
        IIdentityEventPublisher eventPublisher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GenerateNewSecurityCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        if (user.Status)
            return Result.Fail<UserDto>(new BadRequestError("Email already verified"));

        // Generate new verification token
        user.GenerateEmailVerificationToken();
        await _userRepository.UpdateAsync(user);

        // Publish event to send new verification email via Notification Service
        await _eventPublisher.PublishUserRegisteredAsync(user);

        return Result.Ok(_mapper.Map<UserDto>(user));
    }
}

