namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// DTO for a number reservation response.
/// </summary>
public class NumberReservationDto
{
    public Guid NumberId { get; set; }
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public DateTime ReservationExpiresAt { get; set; }
    public int SecondsRemaining { get; set; }
}

