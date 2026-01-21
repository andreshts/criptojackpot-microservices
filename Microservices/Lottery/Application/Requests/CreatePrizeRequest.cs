using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.Requests;

public class CreatePrizeRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal EstimatedValue { get; set; }
    public PrizeType Type { get; set; }
    public int Tier { get; set; } = 1;
    public string MainImageUrl { get; set; } = null!;
    public List<PrizeImageRequest> AdditionalImageUrls { get; set; } = [];
    public Dictionary<string, string> Specifications { get; set; } = [];
    public decimal? CashAlternative { get; set; }
    public bool IsDeliverable { get; set; }

    public bool IsDigital { get; set; }
}
