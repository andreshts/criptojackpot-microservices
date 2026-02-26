using Asp.Versioning;
using AutoMapper;
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
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UserController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Get the current authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var query = new GetCurrentUserQuery { UserGuid = userGuid.Value };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost()]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        var command = _mapper.Map<CreateUserCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetById([FromRoute] long userId)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("get-all-users")]
    public async Task<IActionResult> GetAll([FromQuery] long excludeUserId)
    {
        var query = new GetAllUsersQuery { ExcludeUserId = excludeUserId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPut("{userId:long}")]
    public async Task<IActionResult> Update(long userId, [FromBody] UpdateUserRequest request)
    {
        var command = _mapper.Map<UpdateUserCommand>(request);
        command.UserId = userId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPost("{userId:long}/image/upload-url")]
    public async Task<IActionResult> GenerateUploadUrl(long userId, [FromBody] GenerateUploadUrlRequest request)
    {
        var command = _mapper.Map<GenerateUploadUrlCommand>(request);
        command.UserId = userId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
    
    [Authorize]
    [HttpPatch("update-image-profile")]
    public async Task<IActionResult> UpdateImage([FromBody] UpdateUserImageRequest request)
    {
        var command = _mapper.Map<UpdateUserImageCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        var command = _mapper.Map<RequestPasswordResetCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("reset-password-with-code")]
    public async Task<IActionResult> ResetPasswordWithCode([FromBody] ResetPasswordWithCodeRequest request)
    {
        var command = _mapper.Map<ResetPasswordWithCodeCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
