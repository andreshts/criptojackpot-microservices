using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Domain.Constants;
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
    
    /// <summary>
    /// Creates a new invoice using the CoinPayments API with the specified parameters.
    /// </summary>
    /// <param name="amount">The amount to be billed in the invoice.</param>
    /// <param name="currencyFrom">The currency of the amount to be paid.</param>
    /// <param name="currencyTo">The currency in which the amount will be converted for the recipient, if applicable.</param>
    /// <param name="notes">Optional. Notes or description for the invoice.</param>
    /// <param name="notificationsUrl">Optional. The URL for receiving payment status notifications via webhook.</param>
    /// <param name="cancellationToken">A token to cancel the operation if necessary.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the API response
    /// with the created invoice details.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when an HTTP error occurs while communicating with the CoinPayments API.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// This method constructs the invoice request payload, including details about the billed amount,
    /// associated currency, and optional webhook notification data, before sending it via an HTTP POST request.
    /// Ensure proper authentication headers are provided during the CoinPaymentsProvider initialization.
    /// </remarks>
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
            Currency = currencyFrom,
            Items =
            [
                new InvoiceItem
                {
                    Name = notes ?? "Payment",
                    Quantity = new InvoiceItemQuantity
                    {
                        Value = 1,
                        Type = 2
                    },
                    Amount = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    OriginalAmount = new InvoiceOriginalAmount
                    {
                        CurrencyId = currencyFrom,
                        DisplayValue = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                        Value = amount.ToString("F8", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            ],
            Notes = notes,
            WebhookData = !string.IsNullOrEmpty(notificationsUrl)
                ? new InvoiceWebhookData { NotificationsUrl = notificationsUrl }
                : null
        };

        return SendAsync(HttpMethod.Post, CoinPaymentsEndpoints.CreateInvoice, body, cancellationToken);
    }

    /// <summary>
    /// Retrieves an invoice by its unique identifier from the CoinPayments API.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice to fetch.</param>
    /// <param name="cancellationToken">A token to cancel the operation if necessary.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the API response
    /// with the requested invoice details.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when an HTTP error occurs while communicating with the CoinPayments API.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// The method sends a GET request to the CoinPayments API using the specified invoice identifier.
    /// Ensure valid API credentials are provided during the CoinPaymentProvider initialization.
    /// </remarks>
    public Task<RestResponse> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpMethod.Get,
            string.Format(CoinPaymentsEndpoints.GetInvoiceById, invoiceId),
            null,
            cancellationToken);

    /// <summary>
    /// Retrieves the account balances for supported cryptocurrencies from the CoinPayments API.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to cancel the operation if necessary.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the API response with the list of balances
    /// for supported cryptocurrencies.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when an HTTP error occurs while communicating with the CoinPayments API.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// The method sends a GET request to the CoinPayments endpoint for retrieving account balances.
    /// Ensure that proper authentication headers are set during the initialization of the CoinPaymentProvider.
    /// </remarks>
    public Task<RestResponse> GetBalancesAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, CoinPaymentsEndpoints.GetBalances, null, cancellationToken);

    /// <summary>
    /// Retrieves the list of supported cryptocurrencies and their associated metadata
    /// from the CoinPayments API.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation if necessary.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the API response with the
    /// list of supported cryptocurrencies.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when an HTTP error occurs while communicating with the CoinPayments API.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// The method sends a GET request to the defined endpoint for retrieving the currencies.
    /// Ensure proper authentication headers are provided during the CoinPaymentsProvider initialization.
    /// </remarks>
    public Task<RestResponse> GetCurrenciesAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, CoinPaymentsEndpoints.GetCurrencies, null, cancellationToken);

    /// <summary>
    /// Initiates a withdrawal request to transfer a specified amount of cryptocurrency to a designated address.
    /// </summary>
    /// <param name="amount">The amount of cryptocurrency to be withdrawn, formatted to 8 decimal places.</param>
    /// <param name="currency">The ticker symbol of the cryptocurrency to withdraw (e.g., BTC, ETH).</param>
    /// <param name="address">The target wallet address where the withdrawal will be sent.</param>
    /// <param name="autoConfirm">Indicates whether the withdrawal should be automatically confirmed without manual intervention.</param>
    /// <param name="notificationsUrl">An optional URL to receive transaction notifications via callbacks.</param>
    /// <param name="cancellationToken">A token used to cancel the withdrawal request operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the response of the withdrawal request.</returns>
    /// <exception cref="HttpRequestException">Thrown when an HTTP error occurs while sending the withdrawal request.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <remarks>
    /// This method constructs the request body for the withdrawal operation and sends it to the configured CoinPayments endpoint.
    /// </remarks>
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

        return SendAsync(HttpMethod.Post, CoinPaymentsEndpoints.CreateWithdrawal, body, cancellationToken);
    }
    
    /// <summary>
    /// Sends an HTTP request to the specified endpoint with the provided method, body, and cancellation token.
    /// </summary>
    /// <param name="method">The HTTP method to be used for the request (e.g., GET, POST).</param>
    /// <param name="relativeEndpoint">The API endpoint relative to the base URL.</param>
    /// <param name="body">The payload to be sent with the request, serialized to JSON. Can be null if no body is required.</param>
    /// <param name="cancellationToken">A token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous operation, containing the response of the HTTP request.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <exception cref="HttpRequestException">Thrown if an HTTP error occurs while sending the request.</exception>
    /// <exception cref="Exception">Thrown for general exceptions that occur during the request execution.</exception>
    /// <remarks>
    /// This method is a core utility for sending HTTP requests within the CoinPaymentProvider class. It encapsulates
    /// request creation, error handling, and response wrapping for uniform usage across different API operations.
    /// </remarks>
    private async Task<RestResponse> SendAsync(
        HttpMethod method,
        string relativeEndpoint,
        object? body,
        CancellationToken cancellationToken)
    {
        var restResponse = new RestResponse();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient(ServiceDefaults.CoinPaymentsHttpClient);

            var baseAddress = httpClient.BaseAddress
                ?? throw new InvalidOperationException("CoinPayments HttpClient BaseAddress is not configured");

            var requestUri = new Uri(baseAddress, relativeEndpoint);
        
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            
            var bodyJson = body is not null ? JsonSerializer.Serialize(body, JsonOptions) : string.Empty;
            
            var signature = BuildSignature(method.Method, requestUri.ToString(), timestamp, bodyJson);

            using var request = new HttpRequestMessage(method, requestUri);
            
            request.Headers.Add("X-CoinPayments-Client", _clientId);
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
    
    /// <summary>
    /// Builds a cryptographic signature based on the specified HTTP method, URL, timestamp, and message body.
    /// </summary>
    /// <param name="httpMethod">The HTTP method (e.g., GET, POST) used in the request.</param>
    /// <param name="fullUrl">The full URL of the request, including query parameters if any.</param>
    /// <param name="timestamp">The timestamp in ISO-8601 format used to ensure request validity.</param>
    /// <param name="body">The JSON-serialized body of the request (can be empty if no body is present).</param>
    /// <returns>A Base64-encoded string representing the HMAC-SHA256 signature of the input parameters.</returns>
    private string BuildSignature(string httpMethod, string fullUrl, string timestamp, string body)
    {
        var message = $"\ufeff{httpMethod}\n{fullUrl}\n{_clientId}\n{timestamp}\n{body}";
        
        var keyBytes = Encoding.UTF8.GetBytes(_clientSecret);
        var msgBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(msgBytes);
        
        return Convert.ToBase64String(hash);
    }
}


