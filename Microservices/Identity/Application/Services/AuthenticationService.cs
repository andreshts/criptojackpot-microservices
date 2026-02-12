using AutoMapper;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Service that handles authentication logic.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly JwtConfig _jwtConfig;

    public AuthenticationService(
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IIdentityEventPublisher eventPublisher,
        IMapper mapper,
        IOptions<JwtConfig> jwtConfig)
    {
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _jwtConfig = jwtConfig.Value;
    }

    public bool VerifyPassword(string hash, string password)
    {
        return _passwordHasher.Verify(hash, password);
    }

    public async Task<LoginResultDto> CompleteLoginAsync(
        User user,
        string? deviceInfo,
        string? ipAddress,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var (rawRefreshToken, refreshTokenEntity) = _refreshTokenService.CreateRefreshToken(
            user.Id,
            familyId: null,
            deviceInfo: deviceInfo,
            ipAddress: ipAddress,
            rememberMe: rememberMe);

        user.RegisterSuccessfulLogin();
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishUserLoggedInAsync(user);

        var authResponse = _mapper.Map<AuthResponseDto>(user);
        authResponse.ExpiresIn = _jwtConfig.ExpirationInMinutes * 60;
        authResponse.TwoFactorEnabled = user.TwoFactorEnabled;

        return new LoginResultDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresInMinutes = _jwtConfig.ExpirationInMinutes,
            RequiresTwoFactor = false,
            User = authResponse
        };
    }

    public async Task<LoginResultDto> HandleTwoFactorLoginAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var challengeToken = _jwtTokenService.GenerateAccessToken(user);

        user.RegisterSuccessfulLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResultDto
        {
            AccessToken = challengeToken,
            RefreshToken = string.Empty,
            ExpiresInMinutes = 5,
            RequiresTwoFactor = true,
            User = _mapper.Map<AuthResponseDto>(user)
        };
    }

    public int GetLockoutMinutes(int failedAttempts) => failedAttempts switch
    {
        >= 7 => 30,
        >= 5 => 5,
        _ => 1
    };
}
