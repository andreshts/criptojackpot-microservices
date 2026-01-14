using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class DeleteLotteryDrawCommandHandler : IRequestHandler<DeleteLotteryDrawCommand, Result<LotteryDrawDto>>
{
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeleteLotteryDrawCommandHandler> _logger;

    public DeleteLotteryDrawCommandHandler(
        ILotteryDrawRepository lotteryDrawRepository,
        IMapper mapper,
        ILogger<DeleteLotteryDrawCommandHandler> logger)
    {
        _lotteryDrawRepository = lotteryDrawRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<LotteryDrawDto>> Handle(DeleteLotteryDrawCommand request, CancellationToken cancellationToken)
    {
        var lotteryDraw = await _lotteryDrawRepository.GetLotteryByIdAsync(request.LotteryId);

        if (lotteryDraw is null)
            return Result.Fail<LotteryDrawDto>(new NotFoundError("Lottery not found"));

        try
        {
            var deletedLottery = await _lotteryDrawRepository.DeleteLotteryDrawAsync(lotteryDraw);

            _logger.LogInformation("Lottery {LotteryId} deleted successfully", deletedLottery.LotteryGuid);

            return Result.Ok(_mapper.Map<LotteryDrawDto>(deletedLottery));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete lottery {LotteryId}", request.LotteryId);
            return Result.Fail<LotteryDrawDto>(new InternalServerError("Failed to delete lottery"));
        }
    }
}

