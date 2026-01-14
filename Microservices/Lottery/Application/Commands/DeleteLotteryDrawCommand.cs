using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Commands;

public class DeleteLotteryDrawCommand : IRequest<Result<LotteryDrawDto>>
{
    public Guid LotteryId { get; set; }
}

