using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IReferralService _referralService;
    private readonly IIdentityEventPublisher _eventPublisher;

    private const long DefaultRoleId = 2;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IReferralService referralService,
        IIdentityEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _referralService = referralService;
        _eventPublisher = eventPublisher;
    }

    public async Task<ResultResponse<UserDto?>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Email already registered");

        var referrer = await _referralService.ValidateReferralCodeAsync(request.ReferralCode);
        if (!string.IsNullOrEmpty(request.ReferralCode) && referrer is null)
            return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Invalid referral code");

        var user = CreateUser(request);
        var createdUser = await _userRepository.CreateAsync(user);

        if (referrer != null)
            await _referralService.CreateReferralAsync(referrer, createdUser, request.ReferralCode!);

        await _eventPublisher.PublishUserRegisteredAsync(createdUser);

        return ResultResponse<UserDto?>.Created(createdUser.ToDto());
    }

    private User CreateUser(CreateUserCommand request) => new()
    {
        Email = request.Email,
        Password = _passwordHasher.Hash(request.Password),
        Name = request.Name,
        LastName = request.LastName,
        Phone = request.Phone,
        CountryId = request.CountryId ?? 1,
        StatePlace = string.Empty,
        City = string.Empty,
        SecurityCode = Guid.NewGuid().ToString(),
        Status = false,
        RoleId = DefaultRoleId
    };
}