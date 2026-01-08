using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetReferralStatsQuery : IRequest<Result<UserReferralStatsDto>>
{
    public long UserId { get; set; }
}
