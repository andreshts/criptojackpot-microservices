using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Events;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    private const long DefaultRoleId = 2;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ResultResponse<UserDto?>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Email already registered");

        // Validate referral code if provided
        User? referrer = null;
        if (!string.IsNullOrEmpty(request.ReferralCode))
        {
            referrer = await _userRepository.GetBySecurityCodeAsync(request.ReferralCode);
            if (referrer is null)
                return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Invalid referral code");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var user = CreateUser(request);
            var createdUser = await _userRepository.CreateAsync(user);

            // Publish domain event for referral processing (decoupled)
            await _mediator.Publish(new UserCreatedDomainEvent(createdUser, referrer, request.ReferralCode), cancellationToken);

            // Publish integration event to Kafka for Notification service
            await _eventPublisher.PublishUserRegisteredAsync(createdUser);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("User {UserId} created successfully", createdUser.Id);
            return ResultResponse<UserDto?>.Created(createdUser.ToDto());
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create user for email {Email}", request.Email);
            return ResultResponse<UserDto?>.Failure(ErrorType.InternalServerError, "Failed to create user");
        }
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