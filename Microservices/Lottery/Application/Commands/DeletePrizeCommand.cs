using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Commands;

public class DeletePrizeCommand : IRequest<Result<PrizeDto>>
{
    public Guid PrizeId { get; set; }
}

