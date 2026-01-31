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
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CreateUserCommandHandler> logger,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            return Result.Fail<UserDto>(new BadRequestError("Email already registered"));

        // Validate referral code if provided
        User? referrer = null;
        if (!string.IsNullOrEmpty(request.ReferralCode))
        {
            referrer = await _userRepository.GetByReferralCodeAsync(request.ReferralCode);
            if (referrer is null)
                return Result.Fail<UserDto>(new BadRequestError("Invalid referral code"));
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Create user in Keycloak first
            var keycloakId = await _keycloakAdminService.CreateUserAsync(
                request.Email,
                request.Password,
                request.Name,
                request.LastName,
                emailVerified: false,
                cancellationToken);

            if (string.IsNullOrEmpty(keycloakId))
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Fail<UserDto>(new InternalServerError("Failed to create user in authentication service"));
            }

            var user = _mapper.Map<User>(request);
            user.KeycloakId = keycloakId;
            user.Status = false; // Will be activated after email verification
            user.GenerateEmailVerificationToken(); // Generate token for Notification Service
            
            // Ensure UserGuid is generated 
            if (user.UserGuid == Guid.Empty)
                user.UserGuid = Guid.NewGuid();
            
            var createdUser = await _userRepository.CreateAsync(user);

            // Publish domain event for referral processing 
            await _mediator.Publish(new UserCreatedDomainEvent(createdUser, referrer, request.ReferralCode), cancellationToken);

            // Publish integration event to Kafka for Notification service
            await _eventPublisher.PublishUserRegisteredAsync(createdUser);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("User {UserId} created successfully with KeycloakId {KeycloakId}", createdUser.Id, keycloakId);
            return ResultExtensions.Created(_mapper.Map<UserDto>(createdUser));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create user for email {Email}", request.Email);
            return Result.Fail<UserDto>(new InternalServerError("Failed to create user"));
        }
    }
}