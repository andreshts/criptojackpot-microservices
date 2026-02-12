using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles logout from all devices by revoking all refresh tokens.
/// </summary>
public class LogoutAllDevicesCommandHandler : IRequestHandler<LogoutAllDevicesCommand, Result<int>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LogoutAllDevicesCommandHandler> _logger;

    public LogoutAllDevicesCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<LogoutAllDevicesCommandHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        LogoutAllDevicesCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.UserGuid);
        if (user is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        // Get count of active sessions before revoking
        var activeSessions = await _refreshTokenRepository.GetActiveByUserIdAsync(user.Id);
        var sessionCount = activeSessions.Count;

        if (sessionCount == 0)
        {
            _logger.LogInformation("No active sessions to revoke for user {UserId}", user.Id);
            return Result.Ok(0);
        }

        // Revoke all tokens
        var reason = string.IsNullOrWhiteSpace(request.Reason) 
            ? "logout_all_devices" 
            : request.Reason;
        
        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id, reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Revoked {Count} active sessions for user {UserId}. Reason: {Reason}",
            sessionCount, user.Id, reason);

        // Publish security alert
        await _eventPublisher.PublishSecurityAlertAsync(
            user,
            SecurityAlertType.AllSessionsRevoked,
            $"All sessions ({sessionCount}) have been terminated.",
            null,
            null);

        return Result.Ok(sessionCount);
    }
}

