using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetReferralStatsQueryHandler : IRequestHandler<GetReferralStatsQuery, Result<UserReferralStatsDto>>
{
    private readonly IUserReferralRepository _userReferralRepository;
    private readonly IMapper _mapper;

    public GetReferralStatsQueryHandler(
        IUserReferralRepository userReferralRepository,
        IMapper mapper)
    {
        _userReferralRepository = userReferralRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserReferralStatsDto>> Handle(GetReferralStatsQuery request, CancellationToken cancellationToken)
    {
        var referrals = await _userReferralRepository.GetReferralStatsAsync(request.UserId);
        var referralDtos = _mapper.Map<IEnumerable<UserReferralDto>>(referrals);

        var referralStatsDto = new UserReferralStatsDto
        {
            TotalEarnings = 0,
            LastMonthEarnings = 0,
            Referrals = referralDtos
        };

        return Result.Ok(referralStatsDto);
    }
}
