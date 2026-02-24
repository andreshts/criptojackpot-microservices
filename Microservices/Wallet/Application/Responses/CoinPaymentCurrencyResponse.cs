namespace CryptoJackpot.Wallet.Application.Responses;

/// <summary>
/// Representa una criptomoneda o moneda fiat soportada por el API v2 de CoinPayments.
/// </summary>
public class CoinPaymentCurrencyResponse
{
    /// <summary>ID numérico o compuesto de la moneda (ej: "1", "4:0xabc...")</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Tipo de moneda: crypto | token | fiat</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Símbolo del ticker (ej: BTC, ETH, USDT.ERC20)</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>Nombre legible (ej: Bitcoin, Ethereum)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL del logo SVG</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Número de decimales soportados</summary>
    public int DecimalPlaces { get; set; }

    /// <summary>Ranking de la moneda</summary>
    public int Rank { get; set; }

    /// <summary>Estado: active | underMaintenance | deleted</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Capacidades: payments, conversion, onRamp, offRamp, etc.</summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>Confirmaciones requeridas para considerar el pago válido</summary>
    public int RequiredConfirmations { get; set; }

    /// <summary>Indica si puede recibir pagos actualmente</summary>
    public bool IsEnabledForPayment { get; set; }
}
