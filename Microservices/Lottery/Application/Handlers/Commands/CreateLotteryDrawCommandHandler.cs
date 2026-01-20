using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Utilities;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class CreateLotteryDrawCommandHandler : IRequestHandler<CreateLotteryDrawCommand, Result<LotteryDrawDto>>
{
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateLotteryDrawCommandHandler> _logger;

    public CreateLotteryDrawCommandHandler(
        ILotteryDrawRepository lotteryDrawRepository,
        IPrizeRepository prizeRepository,
        IMapper mapper,
        IEventBus eventBus,
        ILogger<CreateLotteryDrawCommandHandler> logger)
    {
        _lotteryDrawRepository = lotteryDrawRepository;
        _prizeRepository = prizeRepository;
        _mapper = mapper;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<LotteryDrawDto>> Handle(CreateLotteryDrawCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var lotteryDraw = _mapper.Map<LotteryDraw>(request);
            lotteryDraw.LotteryGuid = Guid.NewGuid();
            lotteryDraw.LotteryNo = LotteryNumberGenerator.Generate();
            lotteryDraw.SoldTickets = 0;

            var createdLottery = await _lotteryDrawRepository.CreateLotteryAsync(lotteryDraw);

            if (request.PrizeId.HasValue)
            {
                await _prizeRepository.LinkPrizeToLotteryAsync(request.PrizeId.Value, createdLottery.Id);
            }

            // Publish event to message bus for async number generation
            await _eventBus.Publish(new LotteryCreatedEvent
            {
                LotteryId = createdLottery.LotteryGuid,
                LotteryDbId = createdLottery.Id,
                MinNumber = createdLottery.MinNumber,
                MaxNumber = createdLottery.MaxNumber,
                TotalSeries = createdLottery.TotalSeries,
                // Marketing information
                Title = createdLottery.Title,
                Description = createdLottery.Description,
                TicketPrice = createdLottery.TicketPrice,
                StartDate = createdLottery.StartDate,
                EndDate = createdLottery.EndDate,
                MaxTickets = createdLottery.MaxTickets
            });

            _logger.LogInformation("Lottery {LotteryId} created successfully. Number generation queued.", createdLottery.LotteryGuid);

            return ResultExtensions.Created(_mapper.Map<LotteryDrawDto>(createdLottery));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create lottery with title {Title}", request.Title);
            return Result.Fail<LotteryDrawDto>(new InternalServerError("Failed to create lottery"));
        }
    }
}

