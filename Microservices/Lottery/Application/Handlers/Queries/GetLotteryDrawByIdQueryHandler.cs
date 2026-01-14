using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetLotteryDrawByIdQueryHandler : IRequestHandler<GetLotteryDrawByIdQuery, Result<LotteryDrawDto>>
{
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IMapper _mapper;

    public GetLotteryDrawByIdQueryHandler(
        ILotteryDrawRepository lotteryDrawRepository,
        IMapper mapper)
    {
        _lotteryDrawRepository = lotteryDrawRepository;
        _mapper = mapper;
    }

    public async Task<Result<LotteryDrawDto>> Handle(GetLotteryDrawByIdQuery request, CancellationToken cancellationToken)
    {
        var lotteryDraw = await _lotteryDrawRepository.GetLotteryByIdAsync(request.LotteryId);

        if (lotteryDraw is null)
            return Result.Fail<LotteryDrawDto>(new NotFoundError("Lottery not found"));

        return Result.Ok(_mapper.Map<LotteryDrawDto>(lotteryDraw));
    }
}

