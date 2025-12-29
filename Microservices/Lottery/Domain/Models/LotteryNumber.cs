using CryptoJackpot.Domain.Core.Models;
namespace CryptoJackpot.Lottery.Domain.Models;

public class LotteryNumber : BaseEntity
{
    public Guid Id { get; set; }
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public bool IsAvailable { get; set; }
    public Guid? TicketId { get; set; }

    public virtual LotteryDraw Lottery { get; set; } = null!;
    public virtual Guid? Ticket { get; set; }
}