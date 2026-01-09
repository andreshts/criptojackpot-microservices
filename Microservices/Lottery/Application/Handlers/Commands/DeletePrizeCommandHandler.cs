using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class DeletePrizeCommandHandler : IRequestHandler<DeletePrizeCommand, Result<PrizeDto>>
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeletePrizeCommandHandler> _logger;

    public DeletePrizeCommandHandler(
        IPrizeRepository prizeRepository,
        IMapper mapper,
        ILogger<DeletePrizeCommandHandler> logger)
    {
        _prizeRepository = prizeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PrizeDto>> Handle(DeletePrizeCommand request, CancellationToken cancellationToken)
    {
        var prize = await _prizeRepository.GetPrizeAsync(request.PrizeId);

        if (prize is null)
            return Result.Fail<PrizeDto>(new NotFoundError("Prize not found"));

        try
        {
            var deletedPrize = await _prizeRepository.DeletePrizeAsync(prize);

            _logger.LogInformation("Prize {PrizeId} deleted successfully", deletedPrize.Id);

            return Result.Ok(_mapper.Map<PrizeDto>(deletedPrize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete prize {PrizeId}", request.PrizeId);
            return Result.Fail<PrizeDto>(new InternalServerError("Failed to delete prize"));
        }
    }
}
