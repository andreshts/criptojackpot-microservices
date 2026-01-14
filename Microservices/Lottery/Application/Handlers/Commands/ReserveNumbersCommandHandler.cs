using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

/// <summary>
/// Handler for reserving pre-generated lottery numbers.
/// Numbers are already in the database with Status=Available.
/// This handler updates them to Status=Reserved.
/// </summary>
public class ReserveNumbersCommandHandler : IRequestHandler<ReserveNumbersCommand, Result<List<LotteryNumberDto>>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReserveNumbersCommandHandler> _logger;

    public ReserveNumbersCommandHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        IMapper mapper,
        ILogger<ReserveNumbersCommandHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<LotteryNumberDto>>> Handle(ReserveNumbersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var lottery = await _lotteryDrawRepository.GetLotteryByIdAsync(request.LotteryId);
            if (lottery is null)
                return Result.Fail<List<LotteryNumberDto>>(new NotFoundError("Lottery not found"));

            // Validate numbers are in range
            var invalidNumbers = request.Numbers.Where(n => n < lottery.MinNumber || n > lottery.MaxNumber).ToList();
            if (invalidNumbers.Any())
                return Result.Fail<List<LotteryNumberDto>>(new BadRequestError(
                    $"Numbers out of range: {string.Join(", ", invalidNumbers)}"));

            // Validate series is valid
            if (request.Series < 1 || request.Series > lottery.TotalSeries)
                return Result.Fail<List<LotteryNumberDto>>(new BadRequestError(
                    $"Invalid series. Must be between 1 and {lottery.TotalSeries}"));

            // Find available numbers matching the request
            var availableNumbers = await _lotteryNumberRepository.FindAvailableNumbersAsync(
                request.LotteryId, 
                request.Series, 
                request.Numbers);

            // Check if all requested numbers are available
            var requestedSet = request.Numbers.ToHashSet();
            var availableSet = availableNumbers.Select(n => n.Number).ToHashSet();
            var unavailableNumbers = requestedSet.Except(availableSet).ToList();

            if (unavailableNumbers.Any())
            {
                return Result.Fail<List<LotteryNumberDto>>(new ConflictError(
                    $"Numbers not available: {string.Join(", ", unavailableNumbers)} in series {request.Series}"));
            }

            // Update the numbers to Reserved status
            var now = DateTime.UtcNow;
            foreach (var number in availableNumbers)
            {
                number.Status = NumberStatus.Reserved;
                number.TicketId = request.TicketId;
                number.ReservationExpiresAt = now.AddMinutes(5);
                number.UpdatedAt = now;
            }

            await _lotteryNumberRepository.UpdateRangeAsync(availableNumbers);

            _logger.LogInformation(
                "Reserved {Count} numbers for ticket {TicketId} in lottery {LotteryId}, series {Series}",
                availableNumbers.Count, request.TicketId, request.LotteryId, request.Series);

            var result = _mapper.Map<List<LotteryNumberDto>>(availableNumbers);
            return ResultExtensions.Created(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve numbers for ticket {TicketId}", request.TicketId);
            return Result.Fail<List<LotteryNumberDto>>(new InternalServerError("Failed to reserve numbers"));
        }
    }
}
