using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class CreateUserCommand : IRequest<Result<UserDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public long? CountryId { get; set; }
    public string? ReferralCode { get; set; }
}