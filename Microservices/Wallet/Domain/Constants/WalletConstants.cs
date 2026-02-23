using System.Diagnostics.CodeAnalysis;

namespace CryptoJackpot.Wallet.Domain.Constants;

/// <summary>
/// Configuration keys for the Wallet microservice
/// </summary>
public static class ConfigurationKeys
{
    public const string CoinPaymentsSection = "CoinPayments";
    public const string CoinPaymentsPrivateKey = "CoinPayments:PrivateKey";
    public const string CoinPaymentsPublicKey = "CoinPayments:PublicKey";
    public const string CoinPaymentsBaseUrl = "CoinPayments:BaseUrl";
    
    public const string JwtSettingsSection = "JwtSettings";
    public const string JwtSecretKey = "JwtSettings:SecretKey";
    public const string JwtIssuer = "JwtSettings:Issuer";
    public const string JwtAudience = "JwtSettings:Audience";
    
    public const string CorsAllowedOrigins = "Cors:AllowedOrigins";
    public const string DefaultConnection = "DefaultConnection";
}

/// <summary>
/// Default values for external services
/// </summary>
[SuppressMessage("Design", "S1075:URIs should not be hardcoded", 
    Justification = "Default fallback URL for CoinPayments API, can be overridden via configuration")]
public static class ServiceDefaults
{
    /// <summary>
    /// Default CoinPayments API endpoint URL
    /// </summary>
    public const string CoinPaymentsBaseUrl = "https://a-api.coinpayments.net/api/v1";
    
    /// <summary>
    /// Default HTTP client timeout in seconds
    /// </summary>
    public const int HttpClientTimeoutSeconds = 30;
    
    /// <summary>
    /// Named HttpClient for CoinPayments
    /// </summary>
    public const string CoinPaymentsHttpClient = "CoinPayments";
}

/// <summary>
/// Resilience policy settings
/// </summary>
public static class ResilienceSettings
{
    /// <summary>
    /// Number of retry attempts for transient errors
    /// </summary>
    public const int RetryCount = 3;
    
    /// <summary>
    /// Number of consecutive failures before circuit breaker opens
    /// </summary>
    public const int CircuitBreakerFailureThreshold = 5;
    
    /// <summary>
    /// Duration in seconds the circuit breaker stays open
    /// </summary>
    public const int CircuitBreakerDurationSeconds = 30;
}
