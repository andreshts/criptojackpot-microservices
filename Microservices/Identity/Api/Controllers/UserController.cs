using System.Security.Claims;
using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Application.Requests;
using CryptoJackpot.Infra.IoC;
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

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        // Fast path: user_id claim is present (user already synced with backend DB)
        var userId = User.GetUserId();
        if (userId is not null)
        {
            var query = new GetUserByIdQuery { UserId = userId.Value };
            var result = await _mediator.Send(query);
            return result.ToActionResult();
        }

        // Slow path: user_id missing (self-registered via Keycloak, not yet in backend DB).
        // Use the Keycloak subject (sub) to find or auto-provision the user.
        var keycloakId = User.GetKeycloakSubject();
        var email = User.GetEmail();

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(email))
            return Unauthorized();

        var syncQuery = new SyncCurrentUserQuery
        {
            KeycloakId = keycloakId,
            Email = email,
            FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value
                        ?? User.FindFirst("given_name")?.Value,
            LastName = User.FindFirst(ClaimTypes.Surname)?.Value
                       ?? User.FindFirst("family_name")?.Value,
            EmailVerified = bool.TryParse(
                User.FindFirst("email_verified")?.Value, out var ev) && ev,
        };

        var syncResult = await _mediator.Send(syncQuery);
        return syncResult.ToActionResult();
    }

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

    [HttpPut("{userId:long}")]
    public async Task<IActionResult> Update(long userId, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand
        {
            UserId = userId,
            Name = request.Name,
            LastName = request.LastName,
            Phone = request.Phone
        };

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

    [HttpPatch("update-image-profile")]
    public async Task<IActionResult> UpdateImage([FromBody] UpdateUserImageRequest request)
    {
        var command = new UpdateUserImageCommand
        {
            UserId = request.UserId,
            StorageKey = request.StorageKey
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
