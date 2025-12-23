namespace CryptoJackpot.Identity.Application.DTOs;

public class CountryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Iso2 { get; set; }
    public string? Iso3 { get; set; }
    public string? PhoneCode { get; set; }
    public string? Currency { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? Region { get; set; }
}

