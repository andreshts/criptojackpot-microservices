namespace CryptoJackpot.Order.Domain.Enums;

public enum OrderStatus
{
    Pending,    // Order created, waiting for payment (5 min countdown)
    Completed,  // Payment successful, ticket created
    Expired,    // Order expired (timeout)
    Cancelled   // User cancelled the order
}

