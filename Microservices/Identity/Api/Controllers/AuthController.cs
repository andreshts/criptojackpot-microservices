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
    /// Logout user by clearing auth cookies.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.ClearAuthCookies(_cookieConfig);
        return NoContent();
    }
}
