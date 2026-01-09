using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetPrizeByIdQuery : IRequest<Result<PrizeDto>>
{
    public Guid PrizeId { get; set; }
}

