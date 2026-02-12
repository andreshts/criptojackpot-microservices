using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Api.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CookieConfig _cookieConfig;
    private readonly JwtConfig _jwtConfig;

    public AuthController(
        IMediator mediator,
        IOptions<CookieConfig> cookieConfig,
        IOptions<JwtConfig> jwtConfig)
    {
        _mediator = mediator;
        _cookieConfig = cookieConfig.Value;
        _jwtConfig = jwtConfig.Value;
    }

    /// <summary>
    /// Authenticate user with email and password.
    /// Sets HttpOnly cookies for access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = Request.GetClientIpAddress(),
            UserAgent = Request.GetUserAgent()
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            // Handle LockedError with Retry-After header
            var error = result.Errors.FirstOrDefault();
            if (error is LockedError lockedError)
            {
                Response.Headers.Append("Retry-After", lockedError.RetryAfterSeconds.ToString());
            }
            return result.ToActionResult();
        }

        var loginResult = result.Value;

        // If 2FA is required, set a short-lived challenge cookie instead
        if (loginResult.RequiresTwoFactor)
        {
            Response.SetAccessTokenCookie(
                loginResult.AccessToken,
                _cookieConfig,
                5); // 5 minutes for 2FA challenge

            return Ok(new
            {
                success = true,
                requiresTwoFactor = true,
                data = loginResult.User
            });
        }

        // Set HttpOnly cookies
        Response.SetAuthCookies(
            loginResult.AccessToken,
            loginResult.RefreshToken,
            _cookieConfig,
            _jwtConfig.ExpirationInMinutes,
            request.RememberMe);

        return Ok(new
        {
            success = true,
            data = loginResult.User
        });
    }

    /// <summary>
    /// Logout user by revoking refresh token and clearing cookies.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.GetRefreshToken(_cookieConfig);
        
        var command = new LogoutCommand { RefreshToken = refreshToken };
        await _mediator.Send(command);

        Response.ClearAuthCookies(_cookieConfig);
        return NoContent();
    }

    /// <summary>
    /// Refresh access token using a valid refresh token.
    /// Implements token rotation: old token is revoked, new one issued.
    /// If a revoked token is reused, entire token family is invalidated (security measure).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.GetRefreshToken(_cookieConfig);

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { success = false, message = "Refresh token not found." });
        }

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = Request.GetClientIpAddress(),
            DeviceInfo = Request.GetUserAgent()
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            // Clear cookies on refresh failure - user must login again
            Response.ClearAuthCookies(_cookieConfig);
            return result.ToActionResult();
        }

        var rotationResult = result.Value;

        // Set new HttpOnly cookies with rotated tokens
        Response.SetAuthCookies(
            rotationResult.AccessToken,
            rotationResult.RefreshToken,
            _cookieConfig,
            rotationResult.ExpiresInMinutes,
            rotationResult.IsRememberMe);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Verify 2FA code to complete login.
    /// Challenge token is obtained from HttpOnly cookie set during initial login.
    /// Accepts TOTP code from authenticator app or recovery code.
    /// </summary>
    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify2Fa([FromBody] Verify2FaRequest request)
    {
        var challengeToken = Request.GetAccessToken(_cookieConfig);

        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return Unauthorized(new { success = false, message = "Challenge token not found. Please login again." });
        }

        var command = new Verify2FaChallengeCommand
        {
            ChallengeToken = challengeToken,
            Code = request.Code,
            RecoveryCode = request.RecoveryCode,
            IpAddress = Request.GetClientIpAddress(),
            DeviceInfo = Request.GetUserAgent(),
            RememberMe = request.RememberMe
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            var error = result.Errors.FirstOrDefault();
            if (error is LockedError lockedError)
            {
                Response.Headers.Append("Retry-After", lockedError.RetryAfterSeconds.ToString());
            }
            return result.ToActionResult();
        }

        var loginResult = result.Value;

        // 2FA verified successfully - set full auth cookies
        Response.SetAuthCookies(
            loginResult.AccessToken,
            loginResult.RefreshToken,
            _cookieConfig,
            _jwtConfig.ExpirationInMinutes,
            request.RememberMe);

        return Ok(new
        {
            success = true,
            data = loginResult.User
        });
    }

    /// <summary>
    /// Authenticate or register user via Google OAuth.
    /// Handles existing users, account linking, and new registrations.
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var command = new GoogleLoginCommand
        {
            IdToken = request.IdToken,
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken,
            ExpiresIn = request.ExpiresIn,
            IpAddress = Request.GetClientIpAddress(),
            DeviceInfo = Request.GetUserAgent(),
            RememberMe = request.RememberMe
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            var error = result.Errors.FirstOrDefault();
            if (error is LockedError lockedError)
            {
                Response.Headers.Append("Retry-After", lockedError.RetryAfterSeconds.ToString());
            }
            return result.ToActionResult();
        }

        var loginResult = result.Value;

        // If 2FA is required
        if (loginResult.RequiresTwoFactor)
        {
            Response.SetAccessTokenCookie(
                loginResult.AccessToken,
                _cookieConfig,
                5); // 5 minutes for 2FA challenge

            return Ok(new
            {
                success = true,
                requiresTwoFactor = true,
                data = loginResult.User
            });
        }

        // Set HttpOnly cookies
        Response.SetAuthCookies(
            loginResult.AccessToken,
            loginResult.RefreshToken,
            _cookieConfig,
            _jwtConfig.ExpirationInMinutes,
            request.RememberMe);

        return Ok(new
        {
            success = true,
            data = loginResult.User
        });
    }

    /// <summary>
    /// Logout from all devices by revoking all refresh tokens.
    /// Use when user suspects account compromise.
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAllDevices([FromBody] LogoutAllDevicesRequest? request)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new LogoutAllDevicesCommand
        {
            UserGuid = userGuid.Value,
            Reason = request?.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return result.ToActionResult();

        // Clear cookies on current device
        Response.ClearAuthCookies(_cookieConfig);

        return Ok(new
        {
            success = true,
            message = $"Successfully logged out from {result.Value} device(s).",
            revokedSessions = result.Value
        });
    }
}
