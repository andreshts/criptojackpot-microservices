using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;
namespace CryptoJackpot.Identity.Application.Commands;
/// <summary>
/// Command for user login with email and password.
/// </summary>
public class LoginCommand : IRequest<Result<LoginResultDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
    // Metadata for audit and security
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
