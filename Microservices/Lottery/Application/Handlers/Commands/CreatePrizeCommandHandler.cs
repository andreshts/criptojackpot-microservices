using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class CreatePrizeCommandHandler : IRequestHandler<CreatePrizeCommand, Result<PrizeDto>>
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePrizeCommandHandler> _logger;

    public CreatePrizeCommandHandler(
        IPrizeRepository prizeRepository,
        IMapper mapper,
        ILogger<CreatePrizeCommandHandler> logger)
    {
        _prizeRepository = prizeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PrizeDto>> Handle(CreatePrizeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var prize = _mapper.Map<Prize>(request);
            var createdPrize = await _prizeRepository.CreatePrizeAsync(prize);

            _logger.LogInformation("Prize {PrizeId} created successfully", createdPrize.Id);

            return ResultExtensions.Created(_mapper.Map<PrizeDto>(createdPrize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create prize with name {Name}", request.Name);
            return Result.Fail<PrizeDto>(new InternalServerError("Failed to create prize"));
        }
    }
}