namespace CryptoJackpot.Lottery.Application.DTOs;

public class PrizeImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string Caption { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
