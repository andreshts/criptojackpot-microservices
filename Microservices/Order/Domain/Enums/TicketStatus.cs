namespace CryptoJackpot.Order.Domain.Enums;

public enum TicketStatus
{
    Active,     // Ticket is valid, lottery in progress
    Won,        // Ticket won a prize
    Lost,       // Lottery ended, ticket didn't win
    Refunded    // Ticket was refunded
}
