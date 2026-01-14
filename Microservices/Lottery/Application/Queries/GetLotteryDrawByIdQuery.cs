using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetLotteryDrawByIdQuery : IRequest<Result<LotteryDrawDto>>
{
    public Guid LotteryId { get; set; }
}

