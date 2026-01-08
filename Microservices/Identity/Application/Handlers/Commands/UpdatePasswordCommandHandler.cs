using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UpdatePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        if (!_passwordHasher.Verify(request.CurrentPassword, user.Password))
            return Result.Fail<UserDto>(new BadRequestError("Invalid current password"));

        user.Password = _passwordHasher.Hash(request.NewPassword);
        var updatedUser = await _userRepository.UpdateAsync(user);

        return Result.Ok(updatedUser.ToDto());
    }
}

