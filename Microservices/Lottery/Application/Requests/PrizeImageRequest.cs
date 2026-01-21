namespace CryptoJackpot.Lottery.Application.Requests;

public class PrizeImageRequest
{
    public string ImageUrl { get; set; } = null!;
    public string Caption { get; set; } = null!;
    public int DisplayOrder { get; set; }
}