using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Domain.Interfaces;

namespace CryptoJackpot.Wallet.Application.Providers;

/// <summary>
/// CoinPayments provider for the new API v2 (a-api.coinpayments.net).
/// Authentication: X-CoinPayments-Client-Id + X-CoinPayments-Timestamp + X-CoinPayments-Signature (HMAC-SHA256).
/// Signature = HMAC-SHA256( clientId + timestamp + requestBody , clientSecret )
/// </summary>
public class CoinPaymentProvider : ICoinPaymentProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _clientSecret;
    private readonly string _clientId;
    private readonly IHttpClientFactory _httpClientFactory;

    public CoinPaymentProvider(
        string clientSecret,
        string clientId,
        IHttpClientFactory httpClientFactory)
    {
        _clientSecret      = clientSecret      ?? throw new ArgumentNullException(nameof(clientSecret));
        _clientId          = clientId          ?? throw new ArgumentNullException(nameof(clientId));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    // ── ICoinPaymentProvider ──────────────────────────────────────────────

    public Task<RestResponse> CreateInvoiceAsync(
        decimal amount,
        string currencyFrom,
        string currencyTo,
        string? notes = null,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default)
    {
        var body = new CreateInvoiceRequest
        {
            ClientId = _clientId,
            Amount = new InvoiceAmount
            {
                CurrencyId   = currencyFrom,
                DisplayValue = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                Value        = amount.ToString("F8", System.Globalization.CultureInfo.InvariantCulture)
            },
            Currency   = currencyTo,
            Notes      = notes,
            WebhookData = !string.IsNullOrEmpty(notificationsUrl)
                ? new InvoiceWebhookData { NotificationsUrl = notificationsUrl }
                : null
        };

        return SendAsync(HttpMethod.Post, "merchant/invoices", body, cancellationToken);
    }

    public Task<RestResponse> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, $"merchant/invoices/{invoiceId}", null, cancellationToken);

    public Task<RestResponse> GetBalancesAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "merchant/balance", null, cancellationToken);

    public Task<RestResponse> GetCurrenciesAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "currencies", null, cancellationToken);

    public Task<RestResponse> CreateWithdrawalAsync(
        decimal amount,
        string currency,
        string address,
        bool autoConfirm = false,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default)
    {
        var body = new CreateWithdrawalRequest
        {
            ClientId         = _clientId,
            Amount           = amount.ToString("F8", System.Globalization.CultureInfo.InvariantCulture),
            Currency         = currency,
            Address          = address,
            AutoConfirm      = autoConfirm,
            NotificationsUrl = notificationsUrl
        };

        return SendAsync(HttpMethod.Post, "merchant/withdrawals", body, cancellationToken);
    }

    // ── Core HTTP logic ───────────────────────────────────────────────────

    private async Task<RestResponse> SendAsync(
        HttpMethod method,
        string relativeEndpoint,
        object? body,
        CancellationToken cancellationToken)
    {
        var restResponse = new RestResponse();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient("CoinPayments");

            var baseAddress = httpClient.BaseAddress
                ?? throw new InvalidOperationException("CoinPayments HttpClient BaseAddress is not configured");

            var requestUri = new Uri(baseAddress, relativeEndpoint);
            var timestamp  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var bodyJson   = body is not null ? JsonSerializer.Serialize(body, JsonOptions) : string.Empty;
            var signature  = BuildSignature(timestamp, bodyJson);

            using var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add("X-CoinPayments-Client-Id", _clientId);
            request.Headers.Add("X-CoinPayments-Timestamp", timestamp);
            request.Headers.Add("X-CoinPayments-Signature", signature);

            if (!string.IsNullOrEmpty(bodyJson))
                request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);

            restResponse.Content           = await response.Content.ReadAsStringAsync(cancellationToken);
            restResponse.StatusCode        = response.StatusCode;
            restResponse.StatusDescription = response.ReasonPhrase;
        }
        catch (OperationCanceledException)
        {
            restResponse.StatusCode = HttpStatusCode.RequestTimeout;
            restResponse.Content    = "Request was cancelled";
            throw;
        }
        catch (HttpRequestException ex)
        {
            restResponse.StatusCode = HttpStatusCode.ServiceUnavailable;
            restResponse.Content    = $"Error contacting CoinPayments API: {ex.Message}";
        }
        catch (Exception ex)
        {
            restResponse.StatusCode = HttpStatusCode.InternalServerError;
            restResponse.Content    = $"Unexpected error: {ex.Message}";
            throw;
        }

        return restResponse;
    }

    // ── Signature builder ─────────────────────────────────────────────────

    /// <summary>
    /// HMAC-SHA256( clientId + timestamp + requestBody , clientSecret ) → lowercase hex.
    /// </summary>
    private string BuildSignature(string timestamp, string body)
    {
        var message  = _clientId + timestamp + body;
        var keyBytes = Encoding.UTF8.GetBytes(_clientSecret);
        var msgBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(msgBytes)).ToLowerInvariant();
    }
}