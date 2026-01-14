using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// DTO for available number information.
/// </summary>
public class AvailableNumberDto
{
    public int Number { get; set; }
    public int AvailableSeries { get; set; }
    public int TotalSeries { get; set; }
    public bool IsFullyAvailable => AvailableSeries == TotalSeries;
    public bool IsExhausted => AvailableSeries == 0;
}

