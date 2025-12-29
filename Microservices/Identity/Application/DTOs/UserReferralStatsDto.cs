namespace CryptoJackpot.Identity.Application.DTOs;

public class UserReferralStatsDto
{
    public decimal TotalEarnings { get; set; }
    public decimal LastMonthEarnings { get; set; }
    public IEnumerable<UserReferralDto> Referrals { get; set; } = [];
}
