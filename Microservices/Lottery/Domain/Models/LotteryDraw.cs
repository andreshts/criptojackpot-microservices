using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Domain.Models;

public class LotteryDraw : BaseEntity
{
    public Guid LotteryGuid { get; set; } = Guid.NewGuid();
    public string LotteryNo { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public int TotalSeries { get; set; }

    public decimal TicketPrice { get; set; }
    public int MaxTickets { get; set; }
    public int SoldTickets { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LotteryStatus Status { get; set; }
    public LotteryType Type { get; set; }
    public string Terms { get; set; } = null!;
    public bool HasAgeRestriction { get; set; }
    public int? MinimumAge { get; set; }

    /// <summary>
    /// CoinPayments currency ID used for ticket payments (e.g. "2")
    /// </summary>
    public string CryptoCurrencyId { get; set; } = null!;
    
    /// <summary>
    /// Crypto ticker symbol (e.g. "LTCT", "BTC", "ETH")
    /// </summary>
    public string CryptoCurrencySymbol { get; set; } = null!;

    public List<string> RestrictedCountries { get; set; } = null!;

    public virtual ICollection<Prize> Prizes { get; set; } = null!;
    public virtual ICollection<LotteryNumber> LotteryNumbers { get; set; } = null!;
}