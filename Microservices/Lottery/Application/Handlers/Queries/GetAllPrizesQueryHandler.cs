using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetAllPrizesQueryHandler : IRequestHandler<GetAllPrizesQuery, Result<PagedList<PrizeDto>>>
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;

    public GetAllPrizesQueryHandler(
        IPrizeRepository prizeRepository,
        IMapper mapper)
    {
        _prizeRepository = prizeRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<PrizeDto>>> Handle(GetAllPrizesQuery request, CancellationToken cancellationToken)
    {
        var pagination = new Pagination
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var pagedPrizes = await _prizeRepository.GetAllPrizesAsync(pagination);

        var result = _mapper.Map<PagedList<PrizeDto>>(pagedPrizes);

        return Result.Ok(result);
    }
}
