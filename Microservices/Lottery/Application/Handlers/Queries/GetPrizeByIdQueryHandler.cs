using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetPrizeByIdQueryHandler : IRequestHandler<GetPrizeByIdQuery, Result<PrizeDto>>
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;

    public GetPrizeByIdQueryHandler(
        IPrizeRepository prizeRepository,
        IMapper mapper)
    {
        _prizeRepository = prizeRepository;
        _mapper = mapper;
    }

    public async Task<Result<PrizeDto>> Handle(GetPrizeByIdQuery request, CancellationToken cancellationToken)
    {
        var prize = await _prizeRepository.GetPrizeAsync(request.PrizeId);

        if (prize is null)
            return Result.Fail<PrizeDto>(new NotFoundError("Prize not found"));

        return Result.Ok(_mapper.Map<PrizeDto>(prize));
    }
}
