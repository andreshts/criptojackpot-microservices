using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetAllLotteryDrawsQuery : IRequest<Result<PagedList<LotteryDrawDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

