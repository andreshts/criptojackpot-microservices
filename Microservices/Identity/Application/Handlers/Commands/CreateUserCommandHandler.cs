using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Events;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IRoleRepository  _roleRepository;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IIdentityEventPublisher eventPublisher,
        IMapper mapper,
        IPublisher publisher,
        ILogger<CreateUserCommandHandler> logger,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _publisher = publisher;
        _logger = logger;
        _roleRepository = roleRepository;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _userRepository.ExistsByEmailAsync(request.Email);
            if (existingUser)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return Result.Fail<UserDto>(new ConflictError("A user with this email already exists"));
            }

            // Validate referral code if provided
            User? referrer = null;
            if (!string.IsNullOrWhiteSpace(request.ReferralCode))
            {
                referrer = await _userRepository.GetByReferralCodeAsync(request.ReferralCode);
                if (referrer is null)
                {
                    _logger.LogWarning("Invalid referral code provided: {ReferralCode}", request.ReferralCode);
                    return Result.Fail<UserDto>(new BadRequestError("Invalid referral code"));
                }
            }

            // Create user entity using AutoMapper
            var user = _mapper.Map<User>(request);
            user.PasswordHash = _passwordHasher.Hash(request.Password);
            user.EmailVerified = false;
            
            // Generate email verification token
            var verificationToken = Guid.NewGuid().ToString("N");
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            var defaultRole = await _roleRepository.GetDefaultRoleAsync();
            user.RoleId = defaultRole?.Id ?? 2;

            var createdUser = await _userRepository.CreateAsync(user);

            // Publish domain event for referral processing (must be awaited so
            // ProcessReferralHandler can publish the ReferralCreatedEvent to Kafka
            // before the DI scope is disposed)
            if (referrer is not null)
            {
                await _publisher.Publish(
                    new UserCreatedDomainEvent(createdUser, referrer, request.ReferralCode), 
                    cancellationToken);
                
                _logger.LogDebug(
                    "UserCreatedDomainEvent published for user {UserId} with referrer {ReferrerId}",
                    createdUser.Id, referrer.Id);
            }

            // Fire-and-forget: Publish user registered event for email notification
            _ = _eventPublisher.PublishUserRegisteredAsync(createdUser, verificationToken);

            _logger.LogInformation("User {UserId} created successfully with email {Email}", createdUser.Id, createdUser.Email);

            return ResultExtensions.Created(_mapper.Map<UserDto>(createdUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with email {Email}", request.Email);
            return Result.Fail<UserDto>(new InternalServerError("Failed to create user"));
        }
    }
}

