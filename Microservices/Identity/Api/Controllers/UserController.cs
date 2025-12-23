using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            Name = request.Name,
            LastName = request.LastName,
            Phone = request.Phone,
            CountryId = request.CountryId,
            ReferralCode = request.ReferralCode
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetById(long userId)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? excludeUserId = null)
    {
        var query = new GetAllUsersQuery { ExcludeUserId = excludeUserId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpPut("{userId:long}")]
    public async Task<IActionResult> Update(long userId, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand
        {
            UserId = userId,
            Name = request.Name,
            LastName = request.LastName,
            Phone = request.Phone,
            Password = request.Password
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var command = new UpdatePasswordCommand
        {
            UserId = request.UserId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("password/reset-request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        var command = new RequestPasswordResetCommand { Email = request.Email };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithCodeRequest request)
    {
        var command = new ResetPasswordWithCodeCommand
        {
            Email = request.Email,
            SecurityCode = request.SecurityCode,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("{userId:long}/security-code")]
    public async Task<IActionResult> GenerateSecurityCode(long userId)
    {
        var command = new GenerateNewSecurityCodeCommand { UserId = userId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("{userId:long}/image/upload-url")]
    public async Task<IActionResult> GenerateUploadUrl(long userId, [FromBody] GenerateUploadUrlRequest request)
    {
        var command = new GenerateUploadUrlCommand
        {
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            ExpirationMinutes = request.ExpirationMinutes
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPut("{userId:long}/image")]
    public async Task<IActionResult> UpdateImage(long userId, [FromBody] UpdateUserImageRequest request)
    {
        var command = new UpdateUserImageCommand
        {
            UserId = userId,
            StorageKey = request.StorageKey
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}

