using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// DTO for number status updates (for broadcast).
/// </summary>
public class NumberStatusDto
{
    public Guid NumberId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public NumberStatus Status { get; set; }
}

