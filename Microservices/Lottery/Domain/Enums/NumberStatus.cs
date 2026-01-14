namespace CryptoJackpot.Lottery.Domain.Enums;

public enum NumberStatus
{
    Available,  // Number is available for purchase
    Reserved,   // Number is reserved (in checkout, pending payment)
    Sold        // Number is sold (payment confirmed)
}

