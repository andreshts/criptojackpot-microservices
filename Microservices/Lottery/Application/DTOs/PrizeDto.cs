using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.DTOs;

public class PrizeDto
{
    public Guid Id { get; set; }
    public Guid? LotteryId { get; set; }
    public int Tier { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal EstimatedValue { get; set; }
    public PrizeType Type { get; set; }
    public string MainImageUrl { get; set; } = null!;
    public List<PrizeImageDto> AdditionalImages { get; set; } = [];
    public Dictionary<string, string> Specifications { get; set; } = [];
    public decimal? CashAlternative { get; set; }
    public bool IsDeliverable { get; set; }
    public bool IsDigital { get; set; }
    public Guid? WinnerTicketId { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
