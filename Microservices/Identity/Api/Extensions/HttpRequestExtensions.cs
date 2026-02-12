namespace CryptoJackpot.Identity.Api.Extensions;

/// <summary>
/// Extension methods for extracting client metadata from HTTP requests.
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Gets the client IP address, checking X-Forwarded-For header for proxy scenarios.
    /// </summary>
    public static string? GetClientIpAddress(this HttpRequest request)
    {
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Gets the User-Agent header from the request.
    /// </summary>
    public static string? GetUserAgent(this HttpRequest request)
    {
        return request.Headers.UserAgent.FirstOrDefault();
    }
}

