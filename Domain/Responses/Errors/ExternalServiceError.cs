namespace CryptoJackpot.Domain.Core.Responses.Errors;

/// <summary>
/// Error for external service failures (HTTP 503 Service Unavailable)
/// </summary>
public class ExternalServiceError : ApplicationError
{
    public string ServiceName { get; }

    public ExternalServiceError(string serviceName, string message) 
        : base($"[{serviceName}] {message}", 503)
    {
        ServiceName = serviceName;
        Metadata.Add("ServiceName", serviceName);
    }
}
