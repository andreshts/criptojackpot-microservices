using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        user.Name = request.Name;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.CountryId = request.CountryId;
        user.StatePlace = request.StatePlace;
        user.City = request.City;
        user.Address = request.Address;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return Result.Ok(_mapper.Map<UserDto>(updatedUser));
    }
}
