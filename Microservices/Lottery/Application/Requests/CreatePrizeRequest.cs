using System.ComponentModel.DataAnnotations;
using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.Requests;

public class CreatePrizeRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal EstimatedValue { get; set; }

    [Required]
    public PrizeType Type { get; set; }

    public int Tier { get; set; } = 1;

    [Required]
    public string MainImageUrl { get; set; } = null!;

    public List<string> AdditionalImageUrls { get; set; } = [];

    public Dictionary<string, string> Specifications { get; set; } = [];

    public decimal? CashAlternative { get; set; }

    public bool IsDeliverable { get; set; }

    public bool IsDigital { get; set; }
}
