using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Commands;

public class UpdatePrizeCommand : IRequest<Result<PrizeDto>>
{
    public Guid PrizeId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal EstimatedValue { get; set; }
    public PrizeType Type { get; set; }
    public int Tier { get; set; }
    public string MainImageUrl { get; set; } = null!;
    public List<string> AdditionalImageUrls { get; set; } = [];
    public Dictionary<string, string> Specifications { get; set; } = [];
    public decimal? CashAlternative { get; set; }
    public bool IsDeliverable { get; set; }
    public bool IsDigital { get; set; }
}

