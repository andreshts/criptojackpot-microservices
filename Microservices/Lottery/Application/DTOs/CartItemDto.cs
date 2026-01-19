namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// DTO for a cart item when reserving multiple numbers at once.
/// </summary>
public class CartItemDto
{
    /// <summary>
    /// The number to reserve (e.g., 10, 25, 99).
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// How many series to reserve for this number (default: 1).
    /// </summary>
    public int Quantity { get; set; } = 1;
}
