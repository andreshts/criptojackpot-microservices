using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
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
    private readonly IUserReferralRepository _userReferralRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUserReferralRepository userReferralRepository,
        IPasswordHasher passwordHasher,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _userReferralRepository = userReferralRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
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

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var createdUser = await _userRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create referral relationship if referrer exists
            if (referrer is not null)
            {
                var userReferral = new UserReferral
                {
                    ReferrerId = referrer.Id,
                    ReferredId = createdUser.Id,
                    UsedSecurityCode = request.ReferralCode!
                };

                await _userReferralRepository.CreateUserReferralAsync(userReferral);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "User {UserId} registered with referral code from {ReferrerId}",
                    createdUser.Id, referrer.Id);

                await _eventPublisher.PublishReferralCreatedAsync(referrer, createdUser, request.ReferralCode!);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Generate email verification token and publish event
            var verificationToken = Guid.NewGuid().ToString("N");
            createdUser.EmailVerificationToken = verificationToken;
            createdUser.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            await _userRepository.UpdateAsync(createdUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishUserRegisteredAsync(createdUser, verificationToken);

            _logger.LogInformation("User {UserId} created successfully with email {Email}", createdUser.Id, createdUser.Email);

            return ResultExtensions.Created(_mapper.Map<UserDto>(createdUser));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create user with email {Email}", request.Email);
            return Result.Fail<UserDto>(new InternalServerError("Failed to create user"));
        }
    }
}

