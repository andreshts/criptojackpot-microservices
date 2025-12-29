using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetReferralStatsQueryHandler : IRequestHandler<GetReferralStatsQuery, ResultResponse<UserReferralStatsDto>>
{
    private readonly IUserReferralRepository _userReferralRepository;

    public GetReferralStatsQueryHandler(IUserReferralRepository userReferralRepository)
    {
        _userReferralRepository = userReferralRepository;
    }

    public async Task<ResultResponse<UserReferralStatsDto>> Handle(GetReferralStatsQuery request, CancellationToken cancellationToken)
    {
        var referrals = await _userReferralRepository.GetReferralStatsAsync(request.UserId);

        var referralDtos = referrals.Select(r => new UserReferralDto
        {
            RegisterDate = r.RegisterDate,
            UsedSecurityCode = r.UsedSecurityCode,
            FullName = r.FullName,
            Email = r.Email
        });

        var referralStatsDto = new UserReferralStatsDto
        {
            TotalEarnings = 0,
            LastMonthEarnings = 0,
            Referrals = referralDtos
        };

        return ResultResponse<UserReferralStatsDto>.Ok(referralStatsDto);
    }
}
