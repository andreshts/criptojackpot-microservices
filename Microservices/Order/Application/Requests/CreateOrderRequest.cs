namespace CryptoJackpot.Order.Application.Requests;

public class CreateOrderRequest
{
    public Guid LotteryId { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = [];
}

public class CreateOrderItemRequest
{
    public int Number { get; set; }
    public int Series { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public Guid? LotteryNumberId { get; set; }
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
}

