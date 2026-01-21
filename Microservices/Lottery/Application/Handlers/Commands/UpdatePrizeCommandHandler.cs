using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class UpdatePrizeCommandHandler : IRequestHandler<UpdatePrizeCommand, Result<PrizeDto>>
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePrizeCommandHandler> _logger;

    public UpdatePrizeCommandHandler(
        IPrizeRepository prizeRepository,
        IMapper mapper,
        ILogger<UpdatePrizeCommandHandler> logger)
    {
        _prizeRepository = prizeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PrizeDto>> Handle(UpdatePrizeCommand request, CancellationToken cancellationToken)
    {
        var prize = await _prizeRepository.GetPrizeAsync(request.PrizeId);

        if (prize is null)
            return Result.Fail<PrizeDto>(new NotFoundError("Prize not found"));

        try
        {
            // Update properties
            prize.Name = request.Name;
            prize.Description = request.Description;
            prize.EstimatedValue = request.EstimatedValue;
            prize.Type = request.Type;
            prize.Tier = request.Tier;
            prize.MainImageUrl = request.MainImageUrl;
            prize.Specifications = request.Specifications;
            prize.CashAlternative = request.CashAlternative;
            prize.IsDeliverable = request.IsDeliverable;
            prize.IsDigital = request.IsDigital;
            prize.AdditionalImages = request.AdditionalImageUrls
                .Select(img => new PrizeImage
                {
                    PrizeId = prize.Id,
                    ImageUrl = img.ImageUrl,
                    Caption = img.Caption,
                    DisplayOrder = img.DisplayOrder
                })
                .ToList();

            var updatedPrize = await _prizeRepository.UpdatePrizeAsync(prize);

            _logger.LogInformation("Prize {PrizeId} updated successfully", updatedPrize.Id);

            return Result.Ok(_mapper.Map<PrizeDto>(updatedPrize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update prize {PrizeId}", request.PrizeId);
            return Result.Fail<PrizeDto>(new InternalServerError("Failed to update prize"));
        }
    }
}
