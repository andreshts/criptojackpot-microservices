using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetAllLotteryDrawsQueryHandler : IRequestHandler<GetAllLotteryDrawsQuery, Result<PagedList<LotteryDrawDto>>>
{
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IMapper _mapper;

    public GetAllLotteryDrawsQueryHandler(
        ILotteryDrawRepository lotteryDrawRepository,
        IMapper mapper)
    {
        _lotteryDrawRepository = lotteryDrawRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<LotteryDrawDto>>> Handle(GetAllLotteryDrawsQuery request, CancellationToken cancellationToken)
    {
        var pagination = new Pagination
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var pagedLotteries = await _lotteryDrawRepository.GetAllLotteryDrawsAsync(pagination);

        var result = _mapper.Map<PagedList<LotteryDrawDto>>(pagedLotteries);

        return Result.Ok(result);
    }
}

