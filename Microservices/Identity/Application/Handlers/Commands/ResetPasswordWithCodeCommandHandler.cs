using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class ResetPasswordWithCodeCommandHandler : IRequestHandler<ResetPasswordWithCodeCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public ResetPasswordWithCodeCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(ResetPasswordWithCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        if (string.IsNullOrEmpty(user.SecurityCode) ||
            user.SecurityCode != request.SecurityCode ||
            user.PasswordResetCodeExpiration == null ||
            user.PasswordResetCodeExpiration < DateTime.UtcNow)
        {
            return Result.Fail<UserDto>(new BadRequestError("Invalid or expired security code"));
        }

        if (request.NewPassword != request.ConfirmPassword)
            return Result.Fail<UserDto>(new BadRequestError("Passwords do not match"));

        user.Password = _passwordHasher.Hash(request.NewPassword);
        user.SecurityCode = null;
        user.PasswordResetCodeExpiration = null;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return Result.Ok(_mapper.Map<UserDto>(updatedUser));
    }
}

