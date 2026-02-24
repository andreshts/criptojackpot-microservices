using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class UpdateLotteryDrawCommandHandler : IRequestHandler<UpdateLotteryDrawCommand, Result<LotteryDrawDto>>
{
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateLotteryDrawCommandHandler> _logger;

    public UpdateLotteryDrawCommandHandler(
        ILotteryDrawRepository lotteryDrawRepository,
        IPrizeRepository prizeRepository,
        IMapper mapper,
        ILogger<UpdateLotteryDrawCommandHandler> logger)
    {
        _lotteryDrawRepository = lotteryDrawRepository;
        _prizeRepository = prizeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<LotteryDrawDto>> Handle(UpdateLotteryDrawCommand request, CancellationToken cancellationToken)
    {
        var lotteryDraw = await _lotteryDrawRepository.GetLotteryByGuidAsync(request.LotteryId);

        if (lotteryDraw is null)
            return Result.Fail<LotteryDrawDto>(new NotFoundError("Lottery not found"));

        try
        {
            // Update properties
            lotteryDraw.Title = request.Title;
            lotteryDraw.Description = request.Description;
            lotteryDraw.MinNumber = request.MinNumber;
            lotteryDraw.MaxNumber = request.MaxNumber;
            
            // Calcular TotalSeries automáticamente basado en MaxTickets
            var numbersPerSeries = request.MaxNumber - request.MinNumber + 1;
            lotteryDraw.TotalSeries = request.MaxTickets / numbersPerSeries;
            
            lotteryDraw.TicketPrice = request.TicketPrice;
            lotteryDraw.MaxTickets = request.MaxTickets;
            lotteryDraw.StartDate = request.StartDate;
            lotteryDraw.EndDate = request.EndDate;
            lotteryDraw.Status = request.Status;
            lotteryDraw.Type = request.Type;
            lotteryDraw.Terms = request.Terms;
            lotteryDraw.HasAgeRestriction = request.HasAgeRestriction;
            lotteryDraw.MinimumAge = request.MinimumAge;
            lotteryDraw.CryptoCurrencySymbol = request.CryptoCurrencySymbol;
            lotteryDraw.CryptoCurrencyId = request.CryptoCurrencyId;
            lotteryDraw.RestrictedCountries = request.RestrictedCountries;

            var updatedLottery = await _lotteryDrawRepository.UpdateLotteryDrawAsync(lotteryDraw);

            // Unlink current prize and link new one if provided
            await _prizeRepository.UnlinkPrizesFromLotteryAsync(lotteryDraw.Id);
            
            if (request.PrizeId.HasValue)
            {
                await _prizeRepository.LinkPrizeToLotteryAsync(request.PrizeId.Value, lotteryDraw.Id);
            }
            
            // Recargar la lotería con los premios actualizados
            updatedLottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(updatedLottery.LotteryGuid);

            _logger.LogInformation("Lottery {LotteryId} updated successfully", updatedLottery!.LotteryGuid);

            return Result.Ok(_mapper.Map<LotteryDrawDto>(updatedLottery));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update lottery {LotteryId}", request.LotteryId);
            return Result.Fail<LotteryDrawDto>(new InternalServerError("Failed to update lottery"));
        }
    }
}

