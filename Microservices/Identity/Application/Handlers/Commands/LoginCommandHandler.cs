using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authService;
    private readonly IIdentityEventPublisher _eventPublisher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuthenticationService authService,
        IIdentityEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail<LoginResultDto>(new UnauthorizedError("Invalid email or password"));

        if (user.IsLockedOut)
        {
            var retryAfter = (int)(user.LockoutEndAt!.Value - DateTime.UtcNow).TotalSeconds;
            return Result.Fail<LoginResultDto>(new LockedError(
                "Account is temporarily locked. Try again later.",
                Math.Max(retryAfter, 1)));
        }

        if (user.PasswordHash is null)
            return Result.Fail<LoginResultDto>(new UnauthorizedError(
                "This account uses Google sign-in. Please login with Google."));

        if (!_authService.VerifyPassword(user.PasswordHash, request.Password))
        {
            user.RegisterFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (user.IsLockedOut)
            {
                var lockoutMinutes = _authService.GetLockoutMinutes(user.FailedLoginAttempts);
                await _eventPublisher.PublishUserLockedOutAsync(user, lockoutMinutes, request.IpAddress, request.UserAgent);

                var retryAfter = (int)(user.LockoutEndAt!.Value - DateTime.UtcNow).TotalSeconds;
                return Result.Fail<LoginResultDto>(new LockedError(
                    "Too many failed attempts. Account is temporarily locked.",
                    Math.Max(retryAfter, 1)));
            }

            return Result.Fail<LoginResultDto>(new UnauthorizedError("Invalid email or password"));
        }

        if (!user.EmailVerified)
            return Result.Fail<LoginResultDto>(new ForbiddenError("Please verify your email before logging in."));

        if (user.TwoFactorEnabled)
        {
            var twoFactorResult = await _authService.HandleTwoFactorLoginAsync(user, cancellationToken);
            return Result.Ok(twoFactorResult);
        }

        var loginResult = await _authService.CompleteLoginAsync(
            user, request.UserAgent, request.IpAddress, request.RememberMe, cancellationToken);

        return Result.Ok(loginResult);
    }
}
