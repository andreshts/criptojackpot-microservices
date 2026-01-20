using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

/// <summary>
/// Command to send a marketing email about a new lottery to a user.
/// </summary>
public class SendLotteryMarketingEmailCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string UserLastName { get; set; } = null!;
    
    // Lottery information
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public string LotteryDescription { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxTickets { get; set; }
}
