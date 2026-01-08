using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail<string>(new NotFoundError("User not found"));

        var securityCode = new Random().Next(100000, 999999).ToString();

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            user.SecurityCode = securityCode;
            user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);
            await _userRepository.UpdateAsync(user);

            // Publish event - saved to Outbox in same transaction
            await _eventPublisher.PublishPasswordResetRequestedAsync(user, securityCode);

            // Commit - both update and outbox message committed atomically
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Password reset requested for {Email}", request.Email);
            return Result.Ok("Password reset email sent");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Failed to process password reset for {Email}", request.Email);
            return Result.Fail<string>(new InternalServerError("Failed to process password reset"));
        }
    }
}
