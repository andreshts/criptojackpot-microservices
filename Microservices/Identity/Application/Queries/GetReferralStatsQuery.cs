using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetReferralStatsQuery : IRequest<ResultResponse<UserReferralStatsDto>>
{
    public long UserId { get; set; }
}
